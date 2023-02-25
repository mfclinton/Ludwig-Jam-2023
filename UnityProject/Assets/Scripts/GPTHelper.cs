using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Linq;
using AICore;
using MathUtils;
using System.Text.RegularExpressions;

public class GPTHelper : MonoBehaviour
{
    [SerializeField] TextAsset[] starterWordFiles;
    [SerializeField] TextAsset bannedWordsFile;


    private void Start()
    {
        (InferenceSession session, GPT2Tokenizer tokenizer) = LoadModel();

        string[] starterWords = LoadStarterWords().ToArray();
        HashSet<long> bannedTokens = ComputeBannedTokens(session, tokenizer);
        HashSet<string> bannedWords = LoadBannedWords();
    }

    public (InferenceSession, GPT2Tokenizer) LoadModel()
    {
        string MODEL_PATH_GPT2_OPENAI = @"/StreamingAssets/AIModels/LLMs/GPTDecoders/GPT2OpenAI/model.onnx";
        string modelPath = Application.dataPath + MODEL_PATH_GPT2_OPENAI;
        
        InferenceSession session = new InferenceSession(modelPath);
        GPT2Tokenizer tokenizer = new GPT2Tokenizer();

        return (session, tokenizer);
    }

    #region Data Prep
    public IEnumerable<string> LoadStarterWords()
    {
        LinkedList<string> starterWords = new LinkedList<string>();
        foreach (TextAsset ta in starterWordFiles)
            starterWords.Concat(ta.text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));

        return starterWords;
    }

    public HashSet<long> ComputeBannedTokens(InferenceSession session, GPT2Tokenizer tokenizer)
    {
        IEnumerable<long> tokenIndexes = Enumerable.Range(0, GPT2Tokenizer.MAX_TOKENS).Select(i => (long)i);
        string[] tokens = tokenIndexes.Select((t) => tokenizer.Decode(new long[] { t })).ToArray();
        Regex regex = new Regex("^[a-zA-Z0-9 ']+$");

        HashSet<long> bannedTokens = new HashSet<long>();
        for (long i = 0; i < tokens.Length; i++)
        {
            string token = tokens[i];

            if (!regex.IsMatch(token))
                bannedTokens.Add(i);
        }

        return bannedTokens;
    }

    public HashSet<string> LoadBannedWords()
    {
        string[] lines = bannedWordsFile.text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        HashSet<string> bannedWords = new HashSet<string>();
        foreach (string line in lines)
        {
            // Shift each character in the word back 13 letters in the alphabet
            string bannedWord = "";
            foreach(char c in line)
            {
                char baseChar = char.IsUpper(c) ? 'A' : 'a';
                
                char realChar = (char)((((c - baseChar) - 13 + 26) % 26) + baseChar);
                bannedWord += realChar;
            }

            bannedWords.Add(bannedWord);
        }

        return bannedWords;
    }

    #endregion

    #region Sampling Methods
    
    public IEnumerable<string> SampleReplacementWords(string[] starterWords, int numWords)
    {
        int[] indexes = Probabilities.SampleUniform(0, starterWordFiles.Length, numWords, withReplacement: false);
        return indexes.Select((i) => starterWords[i]);
    }

    public IEnumerable<string> SampleTokens(InferenceSession session, GPT2Tokenizer tokenizer, string context, int topK = 25)
    {
        // Encodes the text to logits
        long[] encodedInputSeq = tokenizer.Encode(context);
        float[] logits = AICore.GPT2Inference.CausalLMPrediction(session, encodedInputSeq);

        // Softmaxes to get token probabilities
        List<(float, int)> probs = AICore.GPT2Inference.ProcessLogits(logits, topK); // TODO: Can we make this more efficient?

        // TODO: Unsure of the other sampling code here

        return probs.Select(tup => tokenizer.Decode(new long[] { tup.Item2 }));
    }

    public string CompleteWord(InferenceSession session, GPT2Tokenizer tokenizer, string context, string currentWord, int maxDepth = 5)
    {
        // We have a maximum of `maxDepth` tokens to complete the word (to prevent it from getting too long/infinite loops)
        for (int i = 0; i < maxDepth; i++)
        {
            // Samples the next token deterministically
            // TODO: Do we want to complete words deterministically?
            string tempContext = context + " " + currentWord;
            string nextToken = SampleTokens(session, tokenizer, context, topK: 1).First();

            // A space, period or comma in the next token indicates the word is over
            if (nextToken[0] == ',' || nextToken[0] == '.' || nextToken[0] == ' ')
                return currentWord;
            else
            {
                currentWord += nextToken;
                context += nextToken;
            }
        }

        return currentWord;
    }

    public IEnumerable<string> GetTopNextTokens(InferenceSession session, GPT2Tokenizer tokenizer, string context, int numNextWords, string currentWord = null, int topK = 25, HashSet<string> bannedWords = null, string[] starterWords)
    {
        IEnumerable<string> topTokens = SampleTokens(session, tokenizer, context, topK);

        Regex regex = new Regex("[^a-zA-Z0-9']");
        LinkedList<string> topNextTokens = new LinkedList<string>();
        foreach (string token in topTokens)
        {
            if (topNextTokens.Count == numNextWords)
                break;

            if(currentWord != null && (token[0] == ',' || token[0] == '.' || token[0] == ' '))
            {
                topNextTokens.AddLast(currentWord);
            }

            // Removes alphanumeric characters and normalizes the characters to lowercase
            string cleanedToken = regex.Replace(token, "").ToLower();

            // Completes the word
            string nextWord = CompleteWord(session, tokenizer, context, cleanedToken);

            // Check if word is banned
            if (bannedWords != null && bannedWords.Contains(nextWord))
                continue;

            topNextTokens.AddLast(nextWord);
        }

        // Once 3 words are found, return them (if not pad to some random words)
        int numTokensNeeded = numNextWords - topNextTokens.Count;
        if (starterWords != null && 0 < numTokensNeeded)
            topNextTokens.Concat(SampleReplacementWords(starterWords, numTokensNeeded));

        return topNextTokens;
    }

    /// <summary>
    /// If the last character is a space, period or comma, get the next words.
    /// Generate first next tokens - only select tokens that are not in the banned token list, sample without replacement from top ~20
    /// Finish words one by one(ordered by highest probability) and check them against a banned word list
    /// Once we get to 3 words, return them randomly
    /// </summary>
    public IEnumerable<string> GetNextWords(InferenceSession session, GPT2Tokenizer tokenizer, string context,
                                            int numNextWords, HashSet<string> bannedWords = null, string[] starterWords = null)
    {
        // Removes whitespace from the end for better performance
        // and gets a list of top tokens
        context = context.TrimEnd();

        IEnumerable<string> topNextTokens = GetTopNextTokens(session, tokenizer, context, numNextWords, bannedWords: bannedWords, starterWords: starterWords);

        return topNextTokens;
    }

    /// <summary>
    /// Case if the user is in a middle of a word and the suggestion will replace their current word.
    /// </summary>
    public IEnumerable<string> GetWordCompletions(InferenceSession session, GPT2Tokenizer tokenizer, string context, int numNextWords, int topK = 50,
                                   HashSet<string> bannedWords = null, string[] starterWords = null)
    {
        (string newContext, string currentWord) = SplitContextAndCurrentWord(context);
        IEnumerable<string> topNextTokens = GetTopNextTokens(session, tokenizer, context, numNextWords, currentWord, topK, bannedWords, starterWords);

        // TODO: Consider rearranging

        return topNextTokens;
    }

    #endregion
    #region Helpers
    public (string, string) SplitContextAndCurrentWord(string context)
    {
        // Split into context and the word to be completed
        int lastSpaceIndex = context.LastIndexOf(' ');

        string currentWord = context.Substring(lastSpaceIndex + 1, context.Length - lastSpaceIndex - 1);
        string newContext = context.Substring(0, lastSpaceIndex);

        return (newContext, currentWord);
    }
    #endregion
}
