using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tweet
{
    public string text { get; private set; }

    public int daysSincePosted { get; set; }

    public string topic1 { get; set; }
    public string topic2 { get; set; }
    public string topic3 { get; set; }

    public int topic1Likes { get; set; }
    public int topic2Likes { get; set; }
    public int topic3Likes { get; set; }

    public TweetUI ui;

    public Tweet(string text, string topic1, string topic2, string topic3)
    {
        this.text = text;
        this.daysSincePosted = 0;
        this.topic1 = topic1;
        this.topic2 = topic2;
        this.topic3 = topic3;
        this.topic1Likes = 0;
        this.topic2Likes = 0;
        this.topic3Likes = 0;
    }

    public void UpdateTweetReactionUI()
    {
        ui.UpdateText(text, daysSincePosted, topic1, topic2, topic3, topic1Likes, topic2Likes, topic3Likes);
    }
}
