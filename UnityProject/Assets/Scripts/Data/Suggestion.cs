using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class Suggestion : MonoBehaviour
{
    string text;
    public TextMeshProUGUI textElement { get; private set; }
    Button button;

    private void Awake()
    {
        textElement = GetComponentInChildren<TextMeshProUGUI>();
        button = GetComponent<Button>();
        TweetManager tweetManager = FindObjectOfType<TweetManager>();

        button.onClick.AddListener(() => tweetManager.UseSuggestion(text));
    }

    public void SetText(string text)
    {
        this.text = text;
        textElement.text = text;
    }
}
