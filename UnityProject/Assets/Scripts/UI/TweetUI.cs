using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TweetUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI dateField;
    [SerializeField] TextMeshProUGUI tweetBody;
    [SerializeField] TweetReactionUI[] tweetReactions;

    public void UpdateText(string text, int daysSincePosted, string topic1, string topic2, string topic3, int topic1Likes, int topic2Likes, int topic3Likes)
    {
        tweetBody.text = text;
        tweetReactions[0].UpdateText(topic1, topic1Likes);
        tweetReactions[1].UpdateText(topic2, topic2Likes);
        tweetReactions[2].UpdateText(topic3, topic3Likes);
    }
}
