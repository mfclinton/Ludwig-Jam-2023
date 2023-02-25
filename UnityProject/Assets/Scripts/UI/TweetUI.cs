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
}
