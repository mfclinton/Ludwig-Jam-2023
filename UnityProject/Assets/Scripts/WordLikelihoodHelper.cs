using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fastenshtein;
using System;
using System.Linq;

public class WordLikelihoodHelper : MonoBehaviour
{
    [SerializeField] TextAsset wordsFile;
    List<(string, double)> words;

    void Awake()
    {
        words = LoadWords();
        int levenshteinDistance = Fastenshtein.Levenshtein.Distance("value1", "value2");

        string[] misspelled_words = new string[] {
            "x",
            "xy",
            "tst",
            "accomodate",
            "recieve",
            "apele",
            "aple",
            "rythm",
            "banjana",
            "ghist",
            "flopeer",
        };

        foreach(string word in misspelled_words)
        {
            IEnumerable<(string, double)> topResults = GetTopWords(word, 2).Take(3);
            string resultStr = string.Join(",", topResults);
            Debug.Log($"{word} | {resultStr}");
        }
    }

    public List<(string, double)> LoadWords()
    {
        IEnumerable<string> starterWords = wordsFile.text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        
        List<(string, double)> words = new List<(string, double)>();
        foreach (string line in starterWords)
        {
            string[] cols = line.Split(", ");

            string word = cols[0];
            double prob = double.Parse(cols[1]);
            words.Add((word, prob));
        }

        return words;
    }

    public IEnumerable<(string, double)> GetTopWords(string word, int maxDist)
    {
        List<(string, double)> validWords = new List<(string, double)>();
        foreach((string w, double p) in words)
        {
            int dist = Fastenshtein.Levenshtein.Distance(word, w);
            if (dist <= maxDist)
                validWords.Add((w, p));
        }

        validWords.OrderByDescending(tup => tup.Item2);

        return validWords;
    }
}
