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
    public int numSuggestions;

    GPTHelper gpt;
    WordLikelihoodHelper wlh;

    TopicManager topicManager;

    // Manage tasks
    bool activeSuggesting;

    private void Start()
    {
        gpt = FindObjectOfType<GPTHelper>();
        wlh = FindObjectOfType<WordLikelihoodHelper>();
        topicManager = FindObjectOfType<TopicManager>();

        ResetTweet();
    }

    public void ResetTweet()
    {
        UpdateTweet("");
    }

    public void UseSuggestion(string chunk = "")
    {
        string newTweet = null;
        if (TweetOnNewWord(currentTweet))
            newTweet = currentTweet + chunk;
        else
        {
            (string context, string _) = GPTHelper.SplitContextAndCurrentWord(currentTweet);
            newTweet = context + " " + chunk;
        }

        UpdateTweet(newTweet);
    }

    public void UseKey(string chunk = "")
    {
        string newTweet = currentTweet + chunk;

        UpdateTweet(newTweet);
    }

    public void UpdateTweet(string newTweet)
    {
        if (maxTweetLength < newTweet.Length)
            return;

        currentTweet = newTweet; // TODO: empty chunk shouldn't trigger a new run
        OnTweetUpdated.Invoke(currentTweet);
        UpdateSuggestions(currentTweet);
    }

    async void UpdateSuggestions(string tweet, bool overrideActive = false)
    {
        if (activeSuggesting && !overrideActive)
            return;
        activeSuggesting = true;

        IEnumerable<string> suggestedWords = null;
        if (tweet.Length == 0)
            suggestedWords = gpt.SampleReplacementWords(numSuggestions);
        else
            suggestedWords = await Task.Run(() => RequestGPTSuggestions(tweet));

        int wordsNeeds = numSuggestions - suggestedWords.Count();
        if (0 < wordsNeeds)
            suggestedWords = suggestedWords.Concat(gpt.SampleReplacementWords(wordsNeeds));

        if(currentTweet != tweet)
        {
            UpdateSuggestions(currentTweet, true);
        }
        else
        {
            OnNewSuggestedTweets.Invoke(suggestedWords.Select(x => x + " ").ToArray());
            activeSuggesting = false;
        }
    }

    IEnumerable<string> RequestGPTSuggestions(string tweet)
    {
        if (TweetOnNewWord(tweet))
            return gpt.GetNextWords(tweet, numSuggestions);
        else
        {
            (string context, string word) = GPTHelper.SplitContextAndCurrentWord(currentTweet);
            return wlh.GetTopWords(word, numSuggestions);
            // return gpt.GetWordCompletions(tweet, numSuggestions);
        }
    }

    public static bool TweetOnNewWord(string tweet)
    {
        return tweet.Length == 0 || tweet.EndsWith(" ") || tweet.EndsWith(".") || tweet.EndsWith(",");
    }
}
