using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AICore;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using MathUtils;
using System.Threading.Tasks;
using System.Linq;

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
    TopicManager topicManager;

    private void Start()
    {
        // ML Setup
        string MODEL_PATH_GPT2_OPENAI = @"/StreamingAssets/AIModels/LLMs/GPTDecoders/GPT2OpenAI/model.onnx";
        string modelPath = Application.dataPath + MODEL_PATH_GPT2_OPENAI;
        session = new InferenceSession(modelPath);
        tokenizer = new GPT2Tokenizer();
        topicManager = FindObjectOfType<TopicManager>();

        numSuggestions = 3;

        ResetTweet();
    }

    public void ResetTweet()
    {
        currentTweet = ""; // TODO
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
        string[] suggestedWords = new string[numSuggestions];
        if (currentTweet.Length == 0)
        {
            int[] indexes = MathUtils.Probabilities.SampleUniform(0, topicManager.activeTopics.Length, numSuggestions, withReplacement: false);
            for (int i = 0; i < numSuggestions; i++)
            {
                suggestedWords[i] = topicManager.activeTopics[indexes[i]].name;
            }
        }
        else
        {
            var wordProbs = await Task.Run(() => GPT2Inference.RecommendedNextWords(session, tokenizer, currentTweet, branching));

            float totalProb = 1f;
            for (int i = 0; i < numSuggestions; i++)
            {
                (float prob, string word) = MathUtils.Probabilities.Sample(wordProbs, totalProb);
                suggestedWords[i] = word;
                totalProb -= prob;
            }
        }

        OnNewSuggestedTweets.Invoke(suggestedWords);
    }
}
