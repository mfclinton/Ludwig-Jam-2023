using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Linq;

namespace AICore
{
    public class RobertaSentiment
    {
        public static float[] ClassificationLMPrediction(InferenceSession session, long[] encodedInputSeq)
        {
            // Create DenseTensors to hold the tokenized input and attention mask
            int inputSeqLength = encodedInputSeq.Length;
            DenseTensor<long> inputSequence = new DenseTensor<Int64>(new int[] { 1, inputSeqLength });
            DenseTensor<long> attentionMask = new DenseTensor<Int64>(new int[] { 1, inputSeqLength });

            for (int j = 0; j < inputSeqLength; j++)
            {
                // Set inputSequence to inputSequenceValues
                inputSequence.SetValue(j, encodedInputSeq[j]);
                attentionMask.SetValue(j, 1);
            }

            List<NamedOnnxValue> inputContainer = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("input_ids", inputSequence),
                    NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask)
                };

            var results = session.Run(inputContainer);
            float[] output = results.AsEnumerable().First().AsTensor<float>().ToArray<float>();

            return output;
        }
    }
}