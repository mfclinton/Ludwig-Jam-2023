using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MathUtils;

namespace AICore
{
    public class GPT2Inference
    {
        static long EOS_TOKEN = 50256;

        /// <summary>
        /// Causal Language Model Prediction
        /// Takes in a ONNX Inference session, tokenizer class, and encoded context
        /// Returns the logits of the model's prediction.
        /// </summary>
        public static float[] CausalLMPrediction(InferenceSession session, long[] encodedInputSeq)
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

            int onetoken_length = output.Length / inputSeqLength;
            int lastTokenIndex = output.Length - onetoken_length;

            float[] logits = output[lastTokenIndex..output.Length];

            return logits;
        }

        /// <summary>
        /// Returns an ordered (probability, tokenIndex) pairs given an array of logits
        /// </summary>
        public static List<(float, int)> ProcessLogits(float[] logits, int topK = 1)
        {
            int[] indexes = Enumerable.Range(0, logits.Length).ToArray();
            IEnumerable<(float, int)> zipped = logits.Zip(indexes, (log, idx) => (log, idx));
            List<(float, int)> orderedZipped = zipped.OrderByDescending(tup => tup.Item1).ToList();

            TopK(orderedZipped, topK);
            
            List<(float, int)> probs = MathUtils.Probabilities.CalculateProbs(orderedZipped);
            
            return probs;
        }

        /// <summary>
        /// Modifies the `orderedZipped` list in place, leaving only the topK tokens
        /// </summary>
        static void TopK(List<(float, int)> orderedZipped, int topK)
        {
            orderedZipped.RemoveRange(topK, orderedZipped.Count() - topK);
        }
    }
}
