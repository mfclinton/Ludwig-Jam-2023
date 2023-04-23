using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class UIManager : MonoBehaviour
{
    // Tweet Helpers
    [SerializeField] Transform suggestedWordsParent;
    [SerializeField] Suggestion suggestionPrefab;
    List<Suggestion> suggestionsPool;

    [SerializeField] TextMeshProUGUI currentTweet;

    // Feed Helpers
    [SerializeField] Transform feedParent;
    [SerializeField] GameObject postPrefab;

    // Topic Helpers
    [SerializeField] TrendingTopicUI[] topicUIs;

    // General
    [SerializeField] Sprite cootsPfp;
    [SerializeField] TextMeshProUGUI totalFollowers;
    [SerializeField] TextMeshProUGUI tweetLength;
    [SerializeField] TextMeshProUGUI dateText;

    // Components
    TweetManager tweetManager;
    GameManager gameManager;
    TopicManager topicManager;

    private void Awake()
    {
        tweetManager = FindObjectOfType<TweetManager>();
        gameManager = FindObjectOfType<GameManager>();
        topicManager = FindObjectOfType<TopicManager>();

        suggestionsPool = new List<Suggestion>();

        tweetManager.OnTweetUpdated += UpdateCurrentTweet;
        tweetManager.OnNewSuggestedTweets += UpdateSuggestedWords;

        gameManager.OnTweetSubmitted += (tweet) =>
        {
            NewPost(tweet);
        };

        gameManager.OnFollowersUpdated += (followersGained, totalFollowers) => UpdateTotalFollowers(totalFollowers);
        gameManager.OnDayEnd += UpdateDate;

        topicManager.OnTrendingUpdated += UpdateTopics;
    }

    private void UpdateCurrentTweet(string text)
    {
        currentTweet.text = text + "|";
        tweetLength.text = $"{text.Length} / {tweetManager.maxTweetLength}";

        for (int i = 0; i < suggestionsPool.Count; i++)
            suggestionsPool[i].SetText("");
    }

    public void UpdateSuggestedWords(string[] words)
    {
        while (suggestedWordsParent.childCount < words.Length)
        {
            Suggestion suggestion = Instantiate(suggestionPrefab, suggestedWordsParent);
            suggestionsPool.Add(suggestion);
        }

        for (int i = 0; i < suggestionsPool.Count; i++)
        {
            Suggestion suggestion = suggestionsPool[i];

            bool isActive = true;
            if (words.Length <= i)
                isActive = false;
            else
                suggestion.SetText(words[i]);

            suggestion.gameObject.SetActive(isActive);
        }
    }

    public void NewPost(Tweet tweet)
    {
        GameObject post = Instantiate(postPrefab, feedParent);
        TweetUI tweetUI = post.GetComponent<TweetUI>();
        tweet.ui = tweetUI;

        Destroy(feedParent.GetChild(0).gameObject);
    }

    public void UpdateTopics(Topic[] topics)
    {
        Topic[] orderedTopics = topics.OrderByDescending(t => t.pops).ToArray();

        for (int i = 0; i < topics.Length; i++)
        {
            Topic t = orderedTopics[i];
            topicUIs[i].UpdateText(t.name, t.pops);
        }
    }

    public void UpdateTotalFollowers(int count)
    {
        totalFollowers.text = count.ToString("N0") + " Followers";
    }

    public void UpdateDate(string date)
    {
        dateText.text = date;
    }
}
