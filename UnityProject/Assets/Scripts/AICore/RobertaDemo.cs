using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AICore;
using Microsoft.ML.OnnxRuntime;
using System.Linq;
using MathUtils;

public class RobertaDemo : MonoBehaviour
{
    [SerializeField] string prompt;
    [SerializeField] bool twitterSentimentDemo;
    [SerializeField] bool englishEmotionDemo;

    // Start is called before the first frame update
    void Start()
    {
        if(twitterSentimentDemo)
            TwitterSentimentDemo();
        if(englishEmotionDemo)
            EnglishEmotionDemo();
    }

    void TwitterSentimentDemo()
    {
        Debug.Log("--- Twitter Sentiment Demo ---");
        string modelPath = Application.dataPath + @"/StreamingAssets/AIModels/LLMs/Roberta/TwitterSentiment/model.onnx";
        float[] probs = GetProbs(modelPath);
        Debug.Log($"Negative: {probs[0]} | Neutral: {probs[1]} | Positive: {probs[2]}");
    }

    void EnglishEmotionDemo()
    {
        Debug.Log("--- English Emotion Demo ---");
        string modelPath = Application.dataPath + @"/StreamingAssets/AIModels/LLMs/Roberta/EnglishEmotion/model.onnx";
        float[] probs = GetProbs(modelPath);
        Debug.Log($"Anger: {probs[0]} | Disgust: {probs[1]} | Fear: {probs[2]} | Joy: {probs[3]} | Neutral: {probs[4]} | Sadness: {probs[5]} | Surprise: {probs[6]}");
    }

    float[] GetProbs(string modelPath)
    {
        var session = new InferenceSession(modelPath);

        var tokenizer = new RobertaTokenizer();
        List<long> encodedInputSeq = tokenizer.Encode(prompt).ToList();
        
        // Test decoder
        var decoded = tokenizer.Decode(encodedInputSeq.ToArray());
        Debug.Assert(decoded.Equals(prompt));

        float[] logits = RobertaSentiment.ClassificationLMPrediction(session, encodedInputSeq.ToArray());
        float[] probs = MathUtils.Probabilities.CalculateProbs(logits);
        return probs;
    }
}
