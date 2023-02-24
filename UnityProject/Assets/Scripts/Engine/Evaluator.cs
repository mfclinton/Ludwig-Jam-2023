using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Linq;

using AICore;
using Microsoft.ML.OnnxRuntime;
using MathUtils;

public class Evaluator
{
    InferenceSession sentimentSession, topicSession;
    Tokenizer sentimentTokenizer;
    RobertaMNLITokenizer topicTokenizer;

    public Evaluator()
    {
        string sentimentModelPath = Application.dataPath + @"/StreamingAssets/AIModels/LLMs/Roberta/TwitterSentiment/model.onnx";
        string topicModelPath = Application.dataPath + @"/StreamingAssets/AIModels/LLMs/Roberta/LargeMNLI/model.onnx";

        sentimentSession = new InferenceSession(sentimentModelPath);
        sentimentTokenizer = new RobertaTokenizer();

        topicSession = new InferenceSession(topicModelPath);
        topicTokenizer = new RobertaMNLITokenizer();
    }

    public Topic MatchTopic(string tweet, Topic[] topics)
    {
        List<float> probs = topics.Select(topic => GetTopicProb(tweet, topic)).ToList();
        Topic topic = topics[probs.IndexOf(probs.Max())];
        return topic;
    }

    private float GetTopicProb(string tweet, Topic topic)
    {
        List<long> encodedInputSeq = topicTokenizer.Encode(tweet, topic.name).ToList();
        float[] logits = RobertaSentiment.ClassificationLMPrediction(topicSession, encodedInputSeq.ToArray()); // 1 x 3
        float[] probs = MathUtils.Probabilities.CalculateProbs(new float[] { logits[0], logits[1] });
        return probs[1];
    }

    private float[] GetSentimentProbs(string tweet)
    {
        List<long> encodedInputSeq = sentimentTokenizer.Encode(tweet).ToList();
        float[] logits = RobertaSentiment.ClassificationLMPrediction(sentimentSession, encodedInputSeq.ToArray());
        float[] probs = MathUtils.Probabilities.CalculateProbs(logits);
        return probs;
    }

    /// <summary>
    /// Evaluates a tweets and returns the change in followers that occurs
    /// </summary>
    public int EvaluateTweet(string tweet, Topic topic)
    {
        float[] probs = GetSentimentProbs(tweet);

        int totalPopsGained = 0;
        for (int i = 0; i < probs.Length; i++)
        {
            float p = probs[i];
            int pop = topic.pops[i];
            int popChange = Mathf.RoundToInt(p * pop);
            totalPopsGained += topic.ModifyPopulation(i, popChange);
        }

        return totalPopsGained;
    }
}
