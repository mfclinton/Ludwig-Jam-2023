using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Data

    int totalFollowers = 0;

    // Components

    TweetManager tweetManager;
    TopicManager topicManager;
    Evaluator eval;

    // Events

    public delegate void OnTweetSubmittedHandler(string tweet, Topic topic, int followersGained, int totalFollowers);
    public event OnTweetSubmittedHandler OnTweetSubmitted;

    private void Awake()
    {
        tweetManager = FindObjectOfType<TweetManager>();
        topicManager = FindObjectOfType<TopicManager>();
        eval = new Evaluator();
    }

    public void SubmitTweet()
    {
        string tweet = tweetManager.currentTweet;
        Topic topic = eval.MatchTopic(tweet, topicManager.activeTopics);
        int followersGained = eval.EvaluateTweet(tweet, topic);
        totalFollowers += followersGained;
        tweetManager.ResetTweet();
        OnTweetSubmitted.Invoke(tweet, topic, followersGained, totalFollowers);
    }
}
