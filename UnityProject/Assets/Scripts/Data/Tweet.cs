using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Tweet
{
    public string text { get; private set; }
    public Topic[] topics;
    int[] topicLikes;
    string datetimeDisplay;

    public TweetUI ui;
    public int daysSincePosted;

    public Tweet(string text, Topic[] currentTopics, string datetimeDisplay)
    {
        this.text = text;
        topics = currentTopics.Select(t => t).ToArray();
        topicLikes = new int[topics.Length];
        this.datetimeDisplay = datetimeDisplay;
        daysSincePosted = 0;
    }

    public int Update(Evaluator eval, int totalFollowers, float dampener)
    {

        // Get the number of tweets for each topic
        // Normalize the tweets per topic
        int[] tweets_each = topics.Select(tt => tt.pops).ToArray();
        float sum_tweets_each = tweets_each.Sum();
        float[] relative_tweets_each = tweets_each.Select(te => te / sum_tweets_each).ToArray();

        // For each topic, compute its probability given the tweet text
        float[] topicProbs = topics.Select(tt => eval.GetTopicProb(text, tt)).ToArray();

        // Add the number of followers gained from each topic
        int[] per_topic_followers = new int[3];
        for (int i = 0; i < 3; i++)
            per_topic_followers[i] = (int)(topicProbs[i] * relative_tweets_each[i] * totalFollowers);

        daysSincePosted += 1;

        // Scale by days since posted (limit this to 3)
        per_topic_followers = per_topic_followers.Select(tf => Mathf.RoundToInt((tf / daysSincePosted) * dampener)).ToArray();

        // Update the total number of followers in the tweet
        for (int i = 0; i < topicLikes.Length; i++)
            topicLikes[i] += Mathf.RoundToInt(per_topic_followers[i]);

        //Update the total follower count
        int totalFollowersGained = per_topic_followers.Sum();
        UpdateTweetReactionUI();

        return totalFollowersGained;
    }

    public void UpdateTweetReactionUI()
    {
        ui.UpdateText(text, datetimeDisplay, topics, topicLikes);
    }
}
