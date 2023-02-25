using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Linq;
using AICore;
using MathUtils;

public class GPT2Demo : MonoBehaviour
{
    private string MODEL_PATH_GPT2_OPENAI = @"/StreamingAssets/AIModels/LLMs/GPTDecoders/GPT2OpenAI/model.onnx";

    // TODO: Replace this with an actual demo. Currently just used to test if ONNX outputs work inside the game. 
    void Start()
    {
        
    }
}
