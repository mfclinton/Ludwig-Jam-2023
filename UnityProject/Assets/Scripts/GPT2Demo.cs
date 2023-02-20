using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Linq;
using TMPro;
using AICore;

public class GPT2Demo : MonoBehaviour
{
    private string MODEL_PATH_GPT2_OPENAI = @"/StreamingAssets/AIModels/LLMs/GPTDecoders/GPT2OpenAI/model.onnx";

    // TODO: Replace this with an actual demo. Currently just used to test if ONNX outputs work inside the game. 
    void Start()
    {
        string MODEL_PATH_GPT2_OPENAI = @"/StreamingAssets/AIModels/LLMs/GPTDecoders/GPT2OpenAI/model.onnx";
        string modelPath = Application.dataPath + MODEL_PATH_GPT2_OPENAI;
        var session = new InferenceSession(modelPath);

        var tokenizer = new GPT2Tokenizer();
        string testInput = "The brown fox jumped over the";

        int n = 10;

        for (int i = 0; i < n; i++)
        {
            string inferenceResult = AICore.GPT2Inference.NextWordPrediction(session, tokenizer, testInput);
            Debug.Log(inferenceResult);
            testInput += inferenceResult;
        }
        Debug.Log(testInput);
    }

    void Update()
    {
        
    }
}
