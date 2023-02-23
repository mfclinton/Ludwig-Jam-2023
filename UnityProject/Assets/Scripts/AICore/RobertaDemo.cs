using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AICore;
using Microsoft.ML.OnnxRuntime;
using System.Linq;
using MathUtils;

public class RobertaDemo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //string modelPath = Application.dataPath + @"/StreamingAssets/AIModels/LLMs/Roberta/TwitterSentiment/model.onnx";
        string modelPath = Application.dataPath + @"/StreamingAssets/AIModels/LLMs/Roberta/EnglishEmotion/model.onnx";
        var session = new InferenceSession(modelPath);

        var tokenizer = new GPT2Tokenizer();
        string testInput = "Oh wow. I didn't know that.";
        List<long> encodedInputSeq = tokenizer.Encode(testInput).ToList();
        float[] logits = RobertaSentiment.ClassificationLMPrediction(session, encodedInputSeq.ToArray());
        float[] probs = MathUtils.Probabilities.CalculateProbs(logits);

        // Debug.Log($"Joy: {probs[0]} | Optimism: {probs[1]} | Anger: {probs[2]} | Sadness: {probs[3]}");
        Debug.Log($"Anger: {probs[0]} | Disgust: {probs[1]} | Fear: {probs[2]} | Joy: {probs[3]} | Neutral: {probs[4]} | Sadness: {probs[5]} | Surprise: {probs[6]}");
        Debug.Log(probs.Sum());
    }

}
