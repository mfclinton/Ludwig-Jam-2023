using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MathUtils;
using System.Linq;

public class TopicManager : MonoBehaviour
{
    [SerializeField] Topic[] topicPool;
    [SerializeField] int numActiveTopics;
    public Topic[] activeTopics { get; private set; }

    public delegate void OnTrendingUpdatedHandler(Topic[] activeTopics);
    public event OnTrendingUpdatedHandler OnTrendingUpdated;

    private void Start()
    {
        UpdateActiveTopics();
    }

    public void UpdateActiveTopics()
    {
        activeTopics = SampleTopics(numActiveTopics);
        OnTrendingUpdated.Invoke(activeTopics);
    }

    public Topic[] SampleTopics(int numSamples)
    {
        int[] indexes = MathUtils.Probabilities.SampleUniform(0, topicPool.Length, numSamples, withReplacement: false);
        return indexes.Select(x => topicPool[x].Clone()).ToArray();
    }
}
