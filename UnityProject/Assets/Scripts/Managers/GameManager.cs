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

        Topic[] activeTopics = topicManager.activeTopics;
        Tweet newTweet = new Tweet(tweetManager.currentTweet, activeTopics, datetimeDisplay);
        tweetQueue.Enqueue(newTweet);
        OnTweetSubmitted.Invoke(newTweet);

        // If the tweetQueue has more than 3 tweets, remove the oldest tweet
        if (tweetQueue.Count > 3)
            tweetQueue.Dequeue();

        // For each tweet in the queue
        HashSet<Topic> distinctTopics = new HashSet<Topic>();
        int totalFollowersGained = 0;
        foreach (Tweet t in tweetQueue)
        {
            totalFollowersGained += t.Update(eval, totalFollowers, dampener);
            distinctTopics = distinctTopics.Concat(t.topics).ToHashSet();
        }

        foreach (Topic distinctTopic in distinctTopics)
            distinctTopic.degradePops();

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
