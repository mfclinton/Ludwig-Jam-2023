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
        static List<(float, int)> ProcessLogits(float[] logits, int topK = 1)
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

        /// <summary>
        /// Constructs the next n most likely tokens in a sequence given an input string
        /// </summary>
        public static List<(float, string)> RecommendedNextWords(InferenceSession session, Tokenizer tokenizer, string input, int branches,
                                                                 float sensitivity = 0.1f, int maxWordCount = 8)
        {
            List<(float, string)> completedWords = new List<(float, string)>();
            Queue<(float, string)> queuedWords = new Queue<(float, string)>();

            List<long> encodedInputSeq = tokenizer.Encode(input).ToList();
            float[] logits = CausalLMPrediction(session, encodedInputSeq.ToArray());
            List<(float, int)> probs = ProcessLogits(logits, topK: branches);

            probs.ForEach(x => queuedWords.Enqueue((x.Item1, tokenizer.Decode(new long[] { x.Item2 }))));

            while (queuedWords.Count() != 0 && completedWords.Count < maxWordCount)
            {
                (float p, string chunk) = queuedWords.Dequeue();

                encodedInputSeq = tokenizer.Encode(input + chunk).ToList();
                logits = CausalLMPrediction(session, encodedInputSeq.ToArray());
                probs = ProcessLogits(logits, topK: branches);

                long[] temp = new long[1];
                foreach ((float prob, int index) in probs)
                {
                    if (index == GPT2Inference.EOS_TOKEN)
                        continue;

                    temp[0] = index;
                    string newChunk = tokenizer.Decode(temp);
                    string newWord = chunk + newChunk;

                    if (newChunk.StartsWith(" "))
                    {
                        completedWords.Add((p * prob, chunk));
                    }
                    else if (newWord.EndsWith(" ") || newWord.EndsWith("."))
                    {
                        completedWords.Add((p * prob, newWord));
                    }
                    else
                    {
                        queuedWords.Enqueue((p * prob, newWord));
                    }
                }
            }

            SortedDictionary<string, float> setOfWordProbs = new SortedDictionary<string, float>();
            completedWords.ForEach(x => {
                if (!setOfWordProbs.ContainsKey(x.Item2))
                    setOfWordProbs[x.Item2] = x.Item1;
                else
                    setOfWordProbs[x.Item2] += x.Item1;
            });
            List<(float, string)> wordProbs = MathUtils.Probabilities.CalculateProbs(setOfWordProbs.Select(x => (x.Value, x.Key)).ToList(), sensitivity);
            return wordProbs;
        }
    }

   
    
}
