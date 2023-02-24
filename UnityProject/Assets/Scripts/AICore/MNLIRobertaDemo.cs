using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AICore;
using Microsoft.ML.OnnxRuntime;
using System.Linq;
using MathUtils;

public class MNLIRobertaDemo : MonoBehaviour
{
    [SerializeField] string prompt;
    [SerializeField] bool useLargeMNLI;
    [SerializeField] bool useBaseMNLI;
    [SerializeField] string[] labels;
    [SerializeField] bool showLogits;

    // Start is called before the first frame update
    void Start()
    {
        if(useBaseMNLI)
            BaseNLIDemo();
        if(useLargeMNLI)
            LargeMNLIDemo();
    }

    void BaseNLIDemo()
    {
        Debug.Log("--- Base NLI Demo ---");
        string modelPath = Application.dataPath + @"/StreamingAssets/AIModels/LLMs/Roberta/BaseNLI/model.onnx";
        PerformInference(modelPath);
    }

    void LargeMNLIDemo()
    {
        Debug.Log("--- Large MNLI Demo ---");
        string modelPath = Application.dataPath + @"/StreamingAssets/AIModels/LLMs/Roberta/LargeMNLI/model.onnx";
        PerformInference(modelPath);
    }

    void PerformInference(string modelPath)
    {
        var session = new InferenceSession(modelPath);
        var tokenizer = new RobertaMNLITokenizer();
        
        // Note: We throw away "Neutral" and take the probability of entailment as the probability of label being true

        foreach(string label in labels)
        {
            List<long> encodedInputSeq = tokenizer.Encode(prompt, label).ToList();
            float[] logits = RobertaSentiment.ClassificationLMPrediction(session, encodedInputSeq.ToArray()); // 1 x 3
            float[] probs = MathUtils.Probabilities.CalculateProbs(new float[] {logits[0], logits[1]});

            if(showLogits)
            {
                Debug.Log("~~~~~~");
                Debug.Log($"Contradiction: {logits[0]} | Neutral: {logits[1]} | Entailment: {logits[2]}");
            }

            Debug.Log($"{label}: {probs[1]}");
        }
    }
}
