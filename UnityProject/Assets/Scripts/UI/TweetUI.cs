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

    public void UpdateText(string text, string datetimeDisplay, Topic[] topics, int[] topicLikes)
    {
        tweetBody.text = text;
        dateField.text = datetimeDisplay;

        for (int i = 0; i < tweetReactions.Length; i++)
            tweetReactions[i].UpdateText(topics[i].name, topicLikes[i]);
    }
}
