using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class UIManager : MonoBehaviour
{
    // Tweet Helpers
    [SerializeField] Transform suggestedWordsParent;
    [SerializeField] Button tweetPrefab;
    List<TextMeshProUGUI> suggestedTweetsPool;

    [SerializeField] TextMeshProUGUI currentTweet;

    // Feed Helpers
    [SerializeField] Transform feedParent;
    [SerializeField] GameObject postPrefab;
    [SerializeField] GameObject commentPrefab;

    // Topic Helpers
    [SerializeField] Transform topicParent;
    [SerializeField] GameObject topicPrefab;
    List<GameObject> topicPool;

    // General
    [SerializeField] Sprite cootsPfp;
    [SerializeField] TextMeshProUGUI totalFollowers;

    // Components
    TweetManager tweetManager;
    GameManager gameManager;
    TopicManager topicManager;

    private void Awake()
    {
        tweetManager = FindObjectOfType<TweetManager>();
        gameManager = FindObjectOfType<GameManager>();
        topicManager = FindObjectOfType<TopicManager>();

        suggestedTweetsPool = new List<TextMeshProUGUI>();
        topicPool = new List<GameObject>();

        tweetManager.OnTweetUpdated += UpdateCurrentTweet;
        tweetManager.OnNewSuggestedTweets += UpdateSuggestedWords;

        gameManager.OnTweetSubmitted += (tweet, topic) =>
        {
            NewPost(tweet);
        };

        gameManager.OnFollowersUpdated += (followersGained, totalFollowers) => UpdateTotalFollowers(totalFollowers);

        topicManager.OnTrendingUpdated += UpdateTopics;
    }

    private void UpdateCurrentTweet(string text)
    {
        currentTweet.text = text + "|";

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
            button.onClick.AddListener(() => {
                string cleanedText = textMeshPro.text.Replace('_', ' '); //TODO: Better soluton for cleaning
                tweetManager.UpdateTweet(cleanedText);
            });
            suggestedTweetsPool.Add(textMeshPro);
        }

        for (int i = 0; i < suggestedTweetsPool.Count; i++)
        {
            bool isActive = true;
            if (words.Length <= i)
                isActive = false;
            else
                suggestedTweetsPool[i].text = words[i].Replace(' ', '_'); ;

            suggestedTweetsPool[i].transform.parent.gameObject.SetActive(isActive);
        }
    }

    public void NewPost(string text)
    {
        GameObject post = Instantiate(postPrefab, feedParent);
        post.GetComponentInChildren<TextMeshProUGUI>().text = text;
        post.GetComponentsInChildren<Image>().First(x => x.name == "ProfilePic").sprite = cootsPfp; // TODO: Make this better
    }

    public void UpdateTopics(Topic[] topics)
    {
        while (topicParent.childCount < topics.Length)
        {
            GameObject topic = Instantiate(topicPrefab, topicParent);
            topicPool.Add(topic);
        }

        for (int i = 0; i < topicPool.Count; i++)
        {
            bool isActive = true;
            if (topics.Length <= i)
            {
                isActive = false;
            }
            else
            {
                TextMeshProUGUI[] texts = topicPool[i].GetComponentsInChildren<TextMeshProUGUI>();
                texts.First(t => t.gameObject.name == "Title").text = topics[i].name;
                texts.First(t => t.gameObject.name == "Info").text = topics[i].TotalFollowers().ToString();
            }

            topicPool[i].SetActive(isActive);
        }
    }

    public void UpdateTotalFollowers(int count)
    {
        totalFollowers.text = count.ToString() + " Followers";
    }
}
