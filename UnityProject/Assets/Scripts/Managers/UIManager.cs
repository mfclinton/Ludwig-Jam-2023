using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Tweet Helpers
    [SerializeField] Transform suggestedWordsParent;
    [SerializeField] Button tweetPrefab;
    List<TextMeshProUGUI> suggestedTweetsPool;
    TweetManager tweetManager;

    [SerializeField] TextMeshProUGUI currentTweet;

    private void Awake()
    {
        tweetManager = FindObjectOfType<TweetManager>();

        suggestedTweetsPool = new List<TextMeshProUGUI>();

        tweetManager.OnTweetUpdated += UpdateCurrentTweet;
        tweetManager.OnNewSuggestedTweets += UpdateSuggestedWords;
    }

    private void UpdateCurrentTweet(string text)
    {
        currentTweet.text = text;

        for (int i = 0; i < suggestedTweetsPool.Count; i++)
        {
            suggestedTweetsPool[i].text = "";
        }
    }

    public void UpdateSuggestedWords(string[] words)
    {
        while (suggestedWordsParent.childCount < words.Length)
        {
            Button button = Instantiate(tweetPrefab, suggestedWordsParent);
            TextMeshProUGUI textMeshPro = button.GetComponentInChildren<TextMeshProUGUI>();
            button.onClick.AddListener(() => tweetManager.UpdateTweet(textMeshPro.text));
            suggestedTweetsPool.Add(textMeshPro);
        }

        for (int i = 0; i < suggestedTweetsPool.Count; i++)
        {
            bool isActive = true;
            if (words.Length <= i)
                isActive = false;
            else
                suggestedTweetsPool[i].text = words[i];

            suggestedTweetsPool[i].transform.parent.gameObject.SetActive(isActive);
        }
    }
}
