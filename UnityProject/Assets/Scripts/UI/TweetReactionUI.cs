using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TweetReactionUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI topicField;
    [SerializeField] TextMeshProUGUI followersField;

    public void UpdateText(string topic, int followers)
    {
        topicField.text = topic;
        followersField.text = followers.ToString("N0");
    }
}
