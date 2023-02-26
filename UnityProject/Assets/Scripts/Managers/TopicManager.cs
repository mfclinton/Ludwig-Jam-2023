using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MathUtils;
using System.Linq;

public class TopicManager : MonoBehaviour
{
    [SerializeField] TextAsset topicsFile;
    [SerializeField] Topic[] topicPool;
    [SerializeField] int numActiveTopics;
    public Topic[] activeTopics { get; private set; }

    public delegate void OnTrendingUpdatedHandler(Topic[] activeTopics);
    public event OnTrendingUpdatedHandler OnTrendingUpdated;

    private void Start()
    {
        InitializeActiveTopics();
    }

    public void InitializeActiveTopics()
    {
        activeTopics = SampleTopics(numActiveTopics);
        UpdateActiveTopics();
    }

    public void UpdateActiveTopics()
    {
        OnTrendingUpdated.Invoke(activeTopics);
    }

    public Topic[] SampleTopics(int numSamples)
    {
        int[] indexes = MathUtils.Probabilities.SampleUniform(0, topicPool.Length, numSamples, withReplacement: false);
        return indexes.Select(x => topicPool[x].Clone()).ToArray();
    }

    void OnValidate()
    {
        string[] lines = topicsFile.text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        List<Topic> newTopics = new List<Topic>();
        foreach(string l in lines)
        {
            newTopics.Add(new Topic(l.Trim(), 0));
        }

        topicPool = newTopics.ToArray();
    }
}
