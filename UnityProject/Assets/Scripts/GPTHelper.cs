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

    // Internal properties
    InferenceSession session;
    GPT2Tokenizer tokenizer;

    string[] starterWords;
    HashSet<long> bannedTokens;
    HashSet<string> bannedWords;

    private void Awake()
    {
        (session, tokenizer) = LoadModel();

        starterWords = LoadStarterWords().ToArray();
        bannedTokens = ComputeBannedTokens();
        bannedWords = LoadBannedWords();

        //string[] test_next_words = new string[]{
        //    "this is an apple. ",
        //    "i have an apple, banana,",
        //    "my best ",
        //    "allen just sent a spaceship to the ",
        //    "i can't ",
        //    "i don't know what to do ",
        //    "how are my cats ",
        //    "mario is a cool ",
        //    "i just ",
        //    "when does ",
        //    "i forgot my ",
        //    "the door creaked ",
        //    "she whispered ",
        //    "the coffee was ",
        //    "i need more ",
        //    "the car won't ",
        //    "i love to ",
        //    "the sky is ",
        //    "my favorite color is ",
        //    "the rain is ",
        //    "he always ",
        //    "i'm allergic to ",
        //    "the movie made me ",
        //    "the cat meowed ",
        //    "the phone rang ",
        //    "the tree swayed ",
        //    "the music played ",
        //    "the pen ran out of ",
        //    "the book fell ",
        //    "the fire crackled ",
        //};

        //string[] test_middle_words = new string[]{
        //    "the door crea",
        //    "my be",
        //    "my n",
        //    "allen just sent a spaceship to th",
        //    "i can",
        //    "i don't know what to d",
        //    "how are my ca",
        //    "mario is a co",
        //    "i jus",
        //    "when doe",
        //    "i forgot m",
        //    "she whispe",
        //    "the coffee wa",
        //    "i need mor",
        //    "the car won",
        //    "i love t",
        //    "the sky i",
        //    "my favorite color i",
        //    "he alw",
        //    "the movie made m",
        //    "the cat me",
        //    "the phone ran",
        //    "the tree swaye",
        //    "the music pla",
        //    "the pen ran out o",
        //    "the book fe",
        //    "the fire cra",
        //};

        //Debug.Log("--Next Word Test--");
        //foreach (string context in test_next_words)
        //{
        //    string result = string.Join(", ", GetNextWords(context, 3));
        //    Debug.Log($"{context} | {result}");
        //}

        //Debug.Log("--Middle Word Test--");
        //foreach (string context in test_middle_words)
        //{
        //    string result = string.Join(", ", GetWordCompletions(context, 3));
        //    Debug.Log($"{context} | {result}");
        //}

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
        IEnumerable<string> starterWords = new LinkedList<string>();
        foreach (TextAsset ta in starterWordFiles)
            starterWords = starterWords.Concat(ta.text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));

        return starterWords;
    }

    public HashSet<long> ComputeBannedTokens()
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
    
    public IEnumerable<string> SampleReplacementWords(int numWords)
    {
        int[] indexes = Probabilities.SampleUniform(0, starterWords.Length, numWords, withReplacement: false);
        return indexes.Select((i) => starterWords[i]);
    }

    public IEnumerable<string> SampleTokens(string context, int topK = 25)
    {
        // Encodes the text to logits
        long[] encodedInputSeq = tokenizer.Encode(context);
        float[] logits = AICore.GPT2Inference.CausalLMPrediction(session, encodedInputSeq);
        foreach(long bannedToken in bannedTokens)
        {
            logits[bannedToken] = -99999;
        }

        // Softmaxes to get token probabilities
        List<(float, int)> probs = AICore.GPT2Inference.ProcessLogits(logits, topK); // TODO: Can we make this more efficient?

        // TODO: Unsure of the other sampling code here

        return probs.Select(tup => tokenizer.Decode(new long[] { tup.Item2 }));
    }

    public string CompleteWord(string context, string currentWord, int maxDepth = 5)
    {
        // We have a maximum of `maxDepth` tokens to complete the word (to prevent it from getting too long/infinite loops)
        for (int i = 0; i < maxDepth; i++)
        {
            // Samples the next token deterministically
            // TODO: Do we want to complete words deterministically?
            string tempContext = context + " " + currentWord;
            string nextToken = SampleTokens(tempContext, topK: 1).First();

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

    public IEnumerable<string> GetTopNextTokens(string context, int numNextWords, bool useCurrentWord = false, int topK = 25)
    {
        IEnumerable<string> topTokens = SampleTokens(context, topK);

        string currentWord = null;
        bool addedSelf = false;
        if(useCurrentWord)
            (context, currentWord) = SplitContextAndCurrentWord(context);

        Regex regex = new Regex("[^a-zA-Z0-9']");
        LinkedList<string> topNextTokens = new LinkedList<string>();
        foreach (string token in topTokens)
        {
            if (topNextTokens.Count == numNextWords)
                break;

            if (useCurrentWord && !addedSelf && (token[0] == ',' || token[0] == '.' || token[0] == ' '))
            {
                topNextTokens.AddLast(currentWord);
                addedSelf = true;
                continue;
            }

            // Removes alphanumeric characters and normalizes the characters to lowercase
            string cleanedToken = regex.Replace(token, "").ToLower();

            // Completes the word
            if (useCurrentWord)
                cleanedToken = currentWord + cleanedToken;
            string nextWord = CompleteWord(context, cleanedToken);

            // Check if word is banned
            if (bannedWords != null && bannedWords.Contains(nextWord))
                continue;

            topNextTokens.AddLast(nextWord);
        }

        // Once 3 words are found, return them (if not pad to some random words)
        int numTokensNeeded = numNextWords - topNextTokens.Count;
        if (starterWords != null && 0 < numTokensNeeded)
            topNextTokens.Concat(SampleReplacementWords(numTokensNeeded));

        return topNextTokens;
    }

    /// <summary>
    /// If the last character is a space, period or comma, get the next words.
    /// Generate first next tokens - only select tokens that are not in the banned token list, sample without replacement from top ~20
    /// Finish words one by one(ordered by highest probability) and check them against a banned word list
    /// Once we get to 3 words, return them randomly
    /// </summary>
    public IEnumerable<string> GetNextWords(string context, int numNextWords)
    {
        // Removes whitespace from the end for better performance
        // and gets a list of top tokens
        context = context.TrimEnd();

        IEnumerable<string> topNextTokens = GetTopNextTokens(context, numNextWords);

        return topNextTokens;
    }

    /// <summary>
    /// Case if the user is in a middle of a word and the suggestion will replace their current word.
    /// </summary>
    public IEnumerable<string> GetWordCompletions(string context, int numNextWords, int topK = 50)
    {
        IEnumerable<string> topNextTokens = GetTopNextTokens(context, numNextWords, true, topK);

        // TODO: Consider rearranging

        return topNextTokens;
    }

    #endregion
    #region Helpers
    public static (string, string) SplitContextAndCurrentWord(string context)
    {
        // Split into context and the word to be completed
        int lastSpaceIndex = context.LastIndexOf(' ');
        if (lastSpaceIndex == -1)
            return ("", context);

        string currentWord = context.Substring(lastSpaceIndex + 1, context.Length - lastSpaceIndex - 1);
        string newContext = context.Substring(0, lastSpaceIndex);

        return (newContext, currentWord);
    }
    #endregion
}
