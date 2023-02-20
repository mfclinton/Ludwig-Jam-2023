using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tweet
{
    public string text { get; private set; }
    public int retweets { get; private set; }
    public int likes { get; private set; }
    public int views { get; private set; }
    public LinkedList<Comment> comments { get; private set; }

    public Tweet(string text)
    {
        this.text = text;
    }

    public void UpdateStats(int retweets, int likes, int views)
    {
        this.retweets += retweets;
        this.likes += likes;
        this.views += views;
    }

    public void AddComment(Comment c)
    {
        this.comments.AddLast(c);
    }
}
