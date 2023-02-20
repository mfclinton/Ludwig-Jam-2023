using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AICore
{
    public class GPT2Inference
    {
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
        /// Constructs the next n most likely tokens in a sequence given an input string
        /// </summary>
        public static string CausalLMDefaultGeneration(InferenceSession session, Tokenizer tokenizer, string input, int n)
        {
            List<long> encodedInputSeq = tokenizer.Encode(input).ToList();

            for (int i = 0; i < n; i++)
            {
                float[] logits = CausalLMPrediction(session, encodedInputSeq.ToArray());
                int[] indexes = Enumerable.Range(0, logits.Length).ToArray();
                IEnumerable<(float, int)> zipped = logits.Zip(indexes, (log, idx) => (log, idx));
                List<(float, int)> orderedZipped = zipped.OrderByDescending(tup => tup.Item1).ToList();

                encodedInputSeq.Add(orderedZipped[0].Item2);
            }

            string result = tokenizer.Decode(encodedInputSeq.ToArray());
            return result;
        }

        /// <summary>
        /// Constructs the next word / finishes the last word in a sequence
        /// </summary>
        public static string NextWordPrediction(InferenceSession session, Tokenizer tokenizer, string input)
        {
            List<long> encodedInputSeq = tokenizer.Encode(input).ToList();
            string newWord = "";

            int MAX_CHUNKS = 5;
            for (int i = 0; i < MAX_CHUNKS; i++)
            {
                float[] logits = CausalLMPrediction(session, encodedInputSeq.ToArray());
                int[] indexes = Enumerable.Range(0, logits.Length).ToArray();
                IEnumerable<(float, int)> zipped = logits.Zip(indexes, (log, idx) => (log, idx));
                List<(float, int)> orderedZipped = zipped.OrderByDescending(tup => tup.Item1).ToList();

                long token = orderedZipped[0].Item2;                
                string chunk = tokenizer.Decode(new long[] { token });

                if (newWord.Length != 0 && (chunk.StartsWith(" ") || chunk.StartsWith(".")))
                {
                    Debug.Log("Exited Before Updating Word");
                    break;
                }

                encodedInputSeq.Add(token);
                newWord += chunk;
                if (newWord.EndsWith(" ") || newWord.EndsWith("."))
                {
                    Debug.Log("Exited After Updating Word");
                    break;
                }
            }

            return newWord;
        }

    }

   
    
}
