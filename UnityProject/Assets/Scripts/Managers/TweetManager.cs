using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AICore;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using MathUtils;
using System.Threading.Tasks;

public class TweetManager : MonoBehaviour
{
    public delegate void OnTweetUpdatedHandler(string newTweet);
    public event OnTweetUpdatedHandler OnTweetUpdated;

    public delegate void OnNewSuggestedTweetsHandler(string[] suggestedTweets);
    public event OnNewSuggestedTweetsHandler OnNewSuggestedTweets;

    public string currentTweet { get; private set; }
    public int numSuggestions { get; private set; }

    InferenceSession session;
    GPT2Tokenizer tokenizer;

    private void Start()
    {
        // ML Setup
        string MODEL_PATH_GPT2_OPENAI = @"/StreamingAssets/AIModels/LLMs/GPTDecoders/GPT2OpenAI/model.onnx";
        string modelPath = Application.dataPath + MODEL_PATH_GPT2_OPENAI;
        session = new InferenceSession(modelPath);
        tokenizer = new GPT2Tokenizer();

        numSuggestions = 3;

        ResetTweet();
    }

    public void ResetTweet()
    {
        currentTweet = "The"; // TODO
        UpdateTweet();
    }

    public void UpdateTweet(string chunk = "")
    {
        currentTweet += chunk; // TODO: empty chunk shouldn't trigger a new run
        OnTweetUpdated.Invoke(currentTweet);
        UpdateSuggestions();
    }

    public async void UpdateSuggestions(int branching = 3)
    {
        var wordProbs = await Task.Run(() => GPT2Inference.RecommendedNextWords(session, tokenizer, currentTweet, branching));
        string[] suggestedWords = new string[numSuggestions];

        float totalProb = 1f;
        for (int i = 0; i < numSuggestions; i++)
        {
            (float prob, string word) = MathUtils.Probabilities.Sample(wordProbs, totalProb);
            suggestedWords[i] = word;
            totalProb -= prob;
        }

        OnNewSuggestedTweets.Invoke(suggestedWords);
    }
}
