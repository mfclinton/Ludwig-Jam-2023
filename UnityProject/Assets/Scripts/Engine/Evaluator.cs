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
    InferenceSession topicSession;
    Tokenizer sentimentTokenizer;
    RobertaMNLITokenizer topicTokenizer;

    public Evaluator()
    {
        string sentimentModelPath = Application.dataPath + @"/StreamingAssets/AIModels/LLMs/Roberta/TwitterSentiment/model.onnx";
        string topicModelPath = Application.dataPath + @"/StreamingAssets/AIModels/LLMs/Roberta/LargeMNLI/model.onnx";

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
    
    public float GetTopicProb(string tweet, Topic topic)
    {
        List<long> encodedInputSeq = topicTokenizer.Encode(tweet, topic.name).ToList();
        float[] logits = RobertaSentiment.ClassificationLMPrediction(topicSession, encodedInputSeq.ToArray()); // 1 x 3
        float[] probs = MathUtils.Probabilities.CalculateProbs(new float[] { logits[0], logits[1] });
        return probs[1];
    }

}
