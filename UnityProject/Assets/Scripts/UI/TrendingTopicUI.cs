using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrendingTopicUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI topicField;
    [SerializeField] TextMeshProUGUI numTweetsField;

    public void UpdateText(string topic, int numTweets)
    {
        topicField.text = topic;
        numTweetsField.text = numTweets.ToString("N0");
    }
}
