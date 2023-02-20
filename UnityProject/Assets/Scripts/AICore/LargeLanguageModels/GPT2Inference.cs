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

        static List<(float, int)> ProcessLogits(float[] logits)
        {
            int[] indexes = Enumerable.Range(0, logits.Length).ToArray();
            IEnumerable<(float, int)> zipped = logits.Zip(indexes, (log, idx) => (log, idx));
            List<(float, int)> orderedZipped = zipped.OrderByDescending(tup => tup.Item1).ToList();

            return orderedZipped;
        }

        /// <summary>
        /// Modifies the `orderedZipped` list in place, leaving only the topK tokens
        /// </summary>
        static void TopK(List<(float, int)> orderedZipped, int topK)
        {
            orderedZipped.RemoveRange(topK, orderedZipped.Count() - topK);
        }

        /// <summary>
        /// Softmaxes the `orderedZipped` list into probabilities 
        /// </summary>
        static List<(float, int)> CalculateProbs(List<(float, int)> orderedZipped, float temperature = 1)
        {
            float max = orderedZipped.Max(tup => tup.Item1);
            float normalizer = orderedZipped.Select(tup => Mathf.Exp((tup.Item1 - max) / temperature)).Sum();
            List<(float, int)> probs = orderedZipped.Select(tup => (Mathf.Exp((tup.Item1 - max) / temperature) / normalizer, tup.Item2)).ToList();

            return probs;
        }

        static List<(float, string)> CalculateProbs(List<(float, string)> orderedZipped, float temperature = 1)
        {
            float max = orderedZipped.Max(tup => tup.Item1);
            float normalizer = orderedZipped.Select(tup => Mathf.Exp((tup.Item1 - max) / temperature)).Sum();
            List<(float, string)> probs = orderedZipped.Select(tup => (Mathf.Exp((tup.Item1 - max) / temperature) / normalizer, tup.Item2)).ToList();

            return probs;
        }

        /// <summary>
        /// Modifies the `orderedZipped` list in place, leaving only the topP percent of tokens
        /// </summary>
        static void TopP(List<(float, int)> orderedZipped, float topP)
        {
            if (topP == 1f)
                return;

            List<(float, int)> probs = CalculateProbs(orderedZipped);

            float cummProb = 0f;
            int index = 0;
            foreach ((float prob, int idx) in probs)
            {
                cummProb += prob;
                index++;

                if (topP <= cummProb)
                    break;
            }

            if (index != orderedZipped.Count())
            {
                orderedZipped.RemoveRange(index, orderedZipped.Count() - index);
            }
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
                List<(float, int)> orderedZipped = ProcessLogits(logits);

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
                List<(float, int)> orderedZipped = ProcessLogits(logits);

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

        /// <summary>
        /// Constructs the next n most likely tokens in a sequence given an input string
        /// </summary>
        public static List<(float, string)> RecommendedNextWords(InferenceSession session, Tokenizer tokenizer, string input, int branches, float sensitivity = 0.1f, int maxWordCount = 20)
        {
            List<(float, string)> completedWords = new List<(float, string)>();
            Queue<(float, string)> queuedWords = new Queue<(float, string)>();

            List<long> encodedInputSeq = tokenizer.Encode(input).ToList();
            float[] logits = CausalLMPrediction(session, encodedInputSeq.ToArray());
            List<(float, int)> orderedZipped = ProcessLogits(logits);

            TopK(orderedZipped, branches);
            List<(float, int)> probs = CalculateProbs(orderedZipped);
            probs.ForEach(x => queuedWords.Enqueue((x.Item1, tokenizer.Decode(new long[] { x.Item2 }))));

            while (queuedWords.Count() != 0 && completedWords.Count < maxWordCount)
            {
                (float p, string chunk) = queuedWords.Dequeue();
                    
                encodedInputSeq = tokenizer.Encode(input + chunk).ToList();
                logits = CausalLMPrediction(session, encodedInputSeq.ToArray());
                orderedZipped = ProcessLogits(logits);
                    
                TopK(orderedZipped, branches);
                probs = CalculateProbs(orderedZipped);

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
            List<(float, string)> wordProbs = CalculateProbs(setOfWordProbs.Select(x => (x.Value, x.Key)).ToList(), sensitivity);
            return wordProbs;
        }

        /// <summary>
        /// Takes a probability distribution of words, and samples from it
        /// </summary>
        public static string SampleWord(List<(float, string)> wordProbs)
        {
            float cummProb = 0f;
            float rng = UnityEngine.Random.value;

            foreach ((float prob, string word) in wordProbs)
            {
                cummProb += prob;

                if (rng <= cummProb)
                    return word;
            }

            throw new Exception("Probability Distribution Not Normalized");
        }
    }

   
    
}
