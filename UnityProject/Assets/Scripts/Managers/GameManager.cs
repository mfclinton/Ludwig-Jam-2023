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

    public delegate void OnTweetSubmittedHandler(string tweet, Topic topic);
    public event OnTweetSubmittedHandler OnTweetSubmitted;

    public delegate void OnFollowersUpdatedHandler(int followersGained, int totalFollowers);
    public event OnFollowersUpdatedHandler OnFollowersUpdated;

    private void Awake()
    {
        tweetManager = FindObjectOfType<TweetManager>();
        topicManager = FindObjectOfType<TopicManager>();
        eval = new Evaluator();
    }

    private void Start()
    {
        OnFollowersUpdated.Invoke(0, totalFollowers);
    }

    public void SubmitTweet()
    {
        string tweet = tweetManager.currentTweet;
        if (tweet.Length == 0)
            return;

        Topic topic = eval.MatchTopic(tweet, topicManager.activeTopics);
       
        int followersGained = eval.EvaluateTweet(tweet, topic);
        UpdateTotalFollowers(followersGained);
        
        tweetManager.ResetTweet();
        OnTweetSubmitted.Invoke(tweet, topic);
        topicManager.UpdateActiveTopics(); // TODO: Move this
    }

    public void UpdateTotalFollowers(int followersGained)
    {
        totalFollowers += followersGained;
        OnFollowersUpdated.Invoke(followersGained, totalFollowers);
    }
}
