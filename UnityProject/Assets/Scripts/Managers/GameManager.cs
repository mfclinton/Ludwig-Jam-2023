using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class GameManager : MonoBehaviour
{
    // Data

    int totalFollowers = 319;
    DateTime datetime = DateTime.Parse("2023-02-25 08:00:00");
    string datetimeDisplay;
    public float dampener;


    // Components

    TweetManager tweetManager;
    TopicManager topicManager;
    Evaluator eval;

    // Events

    public delegate void OnTweetSubmittedHandler(Tweet tweet);
    public event OnTweetSubmittedHandler OnTweetSubmitted;

    public delegate void OnFollowersUpdatedHandler(int followersGained, int totalFollowers);
    public event OnFollowersUpdatedHandler OnFollowersUpdated;

    public delegate void OnDayEndHandler(string dateTime);
    public event OnDayEndHandler OnDayEnd;

    // Tweet objects to display in the UI
    Queue<Tweet> tweetQueue = new Queue<Tweet>(3);
    

    private void Awake()
    {
        tweetManager = FindObjectOfType<TweetManager>();
        topicManager = FindObjectOfType<TopicManager>();
        eval = new Evaluator();
    }

    private void Start()
    {
        OnFollowersUpdated.Invoke(0, totalFollowers);
        // Format datetime in full day of week HH:MM AM/PM format. Example: Saturday 4:00PM
        datetimeDisplay = datetime.ToString("dddd h:mm tt");
        OnDayEnd.Invoke(datetimeDisplay);
    }

    public void SubmitTweet()
    {
        string tweet = tweetManager.currentTweet;
        if (tweet.Length == 0)
            return;

        var activeTopics = topicManager.activeTopics;
        Tweet newTweet = new Tweet(tweetManager.currentTweet, activeTopics[0].name, activeTopics[1].name, activeTopics[2].name, datetimeDisplay);
        tweetQueue.Enqueue(newTweet);
        OnTweetSubmitted.Invoke(newTweet);

        // If the tweetQueue has more than 3 tweets, remove the oldest tweet
        if (tweetQueue.Count > 3)
            tweetQueue.Dequeue();

        // Get the trending topics
        Topic[] trendingTopics = topicManager.activeTopics;

        // Get the number of tweets for each topic
        int[] tweets_each = new int[3];
        for (int i = 0; i < 3; i++)
        {
            tweets_each[i] = trendingTopics[i].pops;
        }

        // For each tweet in the queue
        int totalFollowersGained = 0;
        foreach (Tweet t in tweetQueue)
        {
            // For each topic, compute its probability given the tweet text
            float[] topicProbs = new float[3];
            for (int i = 0; i < 3; i++)
            {
                topicProbs[i] = eval.GetTopicProb(t.text, trendingTopics[i]);
            }

            float sum = topicProbs.Sum();
            topicProbs = topicProbs.Select(x => x / sum).ToArray();

            // Sum of tweets_each
            float sum_tweets_each = 0;
            for (int i = 0; i < 3; i++)
            {
                sum_tweets_each += tweets_each[i];
            }

            // Normalize the tweets per topic
            float[] relative_tweets_each = new float[3];
            for (int i = 0; i < 3; i++)
            {
                relative_tweets_each[i] = tweets_each[i] / sum_tweets_each;
            }

            // Add the number of followers gained from each topic
            int[] per_topic_followers = new int[3];
            for (int i = 0; i < 3; i++)
            {
                per_topic_followers[i] = (int)(topicProbs[i] * relative_tweets_each[i] * totalFollowers);
            }

            t.daysSincePosted += 1;
            // Scale by days since posted (limit this to 3)
            for (int i = 0; i < 3; i++)
            {
                per_topic_followers[i] = (int)((per_topic_followers[i] / t.daysSincePosted) * dampener);
            }

            // Update the total number of followers in the tweet
            t.topic1Likes += Mathf.RoundToInt(per_topic_followers[0]);
            t.topic2Likes += Mathf.RoundToInt(per_topic_followers[1]);
            t.topic3Likes += Mathf.RoundToInt(per_topic_followers[2]);

            //Update the total follower count
            foreach (int i in per_topic_followers)
            {
                totalFollowersGained += i;
            }

            t.UpdateTweetReactionUI();
        }

        // TODO: Update current trending topics
        // With 1/3 probability, choose to discard a trending topic and sample a new one
        // Make sure to call Topic.degradePops() on each non discarded topic
        foreach (Topic oldTopic in topicManager.activeTopics)
            oldTopic.degradePops();
        topicManager.UpdateActiveTopics();

        //Advance time by 2 hours
        datetime = datetime.AddHours(2);
        datetimeDisplay = datetime.ToString("dddd h:mm tt");

        //Update total followers
        UpdateTotalFollowers(totalFollowersGained);

        tweetManager.ResetTweet();
        OnDayEnd.Invoke(datetimeDisplay);
    }

    public void UpdateTotalFollowers(int followersGained)
    {
        totalFollowers += followersGained;
        OnFollowersUpdated.Invoke(followersGained, totalFollowers);
    }
}
