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
    public int maxTweetLength;

    public delegate void OnTweetUpdatedHandler(string newTweet);
    public event OnTweetUpdatedHandler OnTweetUpdated;

    public delegate void OnNewSuggestedTweetsHandler(string[] suggestedTweets);
    public event OnNewSuggestedTweetsHandler OnNewSuggestedTweets;

    public string currentTweet { get; private set; }
    public int numSuggestions { get; private set; }

    GPTHelper gpt;
    TopicManager topicManager;

    // Manage tasks
    bool activeSuggesting;

    private void Start()
    {
        gpt = FindObjectOfType<GPTHelper>();
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
        string newTweet = currentTweet + chunk;
        if (maxTweetLength < newTweet.Length)
            return;

        currentTweet = newTweet; // TODO: empty chunk shouldn't trigger a new run
        OnTweetUpdated.Invoke(currentTweet);
        UpdateSuggestions(currentTweet);
    }

    async void UpdateSuggestions(string tweet, int branching = 3, bool overrideActive = false)
    {
        if (activeSuggesting && !overrideActive)
            return;
        activeSuggesting = true;

        IEnumerable<string> suggestedWords = null;
        if (tweet.Length == 0)
            suggestedWords = gpt.SampleReplacementWords(numSuggestions);
        else
            suggestedWords = await Task.Run(() => RequestGPTSuggestions(tweet));

        if(currentTweet != tweet)
        {
            UpdateSuggestions(currentTweet, branching, true);
        }
        else
        {
            OnNewSuggestedTweets.Invoke(suggestedWords.Select(x => x + " ").ToArray());
            activeSuggesting = false;
        }
    }

    IEnumerable<string> RequestGPTSuggestions(string tweet)
    {
        if (tweet.EndsWith(" ") || tweet.EndsWith(".") || tweet.EndsWith(","))
            return gpt.GetNextWords(tweet, numSuggestions);
        else
            return gpt.GetWordCompletions(tweet, numSuggestions);
    }
}
