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

        //string[] misspelled_words = new string[] {
        //    "x",
        //    "xy",
        //    "tst",
        //    "accomodate",
        //    "recieve",
        //    "apele",
        //    "aple",
        //    "rythm",
        //    "banjana",
        //    "ghist",
        //    "flopeer",
        //};

        //foreach(string word in misspelled_words)
        //{
        //    IEnumerable<(string, double)> topResults = GetTopWords(word, 2).Take(3);
        //    string resultStr = string.Join(",", topResults);
        //    Debug.Log($"{word} | {resultStr}");
        //}
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

    public IEnumerable<string> GetTopWords(string word, int wordsNeeded, int maxDist = 3)
    {
        List<(string, double)>[] validWords = new List<(string, double)>[maxDist + 1];
        for (int i = 0; i < validWords.Length; i++)
            validWords[i] = new List<(string, double)>();

        foreach ((string w, double p) in words)
        {
            int dist = Fastenshtein.Levenshtein.Distance(word, w);
            if (dist <= maxDist)
                validWords[dist].Add((w, p * dist));
        }

        IEnumerable<string> returnedWords = new List<string>();
        for (int i = 0; i < validWords.Length; i++)
        {
            int wordsRemaining = wordsNeeded - returnedWords.Count();
            var distIWords = validWords[i].OrderByDescending(tup => tup.Item2);
            returnedWords = returnedWords.Concat(distIWords.Take(wordsRemaining).Select(tup => tup.Item1));
        }

        return returnedWords;
    }
}
