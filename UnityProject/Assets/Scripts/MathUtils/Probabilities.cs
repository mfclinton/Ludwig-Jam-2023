using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace MathUtils
{
    public class Probabilities
    {
        /// <summary>
        /// Softmaxes the `orderedZipped` list into probabilities 
        /// </summary>
        public static List<(float, int)> CalculateProbs(List<(float, int)> orderedZipped, float temperature = 1)
        {
            float max = orderedZipped.Max(tup => tup.Item1);
            float normalizer = orderedZipped.Select(tup => Mathf.Exp((tup.Item1 - max) / temperature)).Sum();
            List<(float, int)> probs = orderedZipped.Select(tup => (Mathf.Exp((tup.Item1 - max) / temperature) / normalizer, tup.Item2)).ToList();

            return probs;
        }

        public static List<(float, string)> CalculateProbs(List<(float, string)> orderedZipped, float temperature = 1)
        {
            float max = orderedZipped.Max(tup => tup.Item1);
            float normalizer = orderedZipped.Select(tup => Mathf.Exp((tup.Item1 - max) / temperature)).Sum();
            List<(float, string)> probs = orderedZipped.Select(tup => (Mathf.Exp((tup.Item1 - max) / temperature) / normalizer, tup.Item2)).ToList();

            return probs;
        }

        public static float[] CalculateProbs(float[] logits, float temperature = 1)
        {
            float max = logits.Max(x => x);
            float normalizer = logits.Select(x => Mathf.Exp((x - max) / temperature)).Sum();
            float[] probs = logits.Select(x => Mathf.Exp((x - max) / temperature) / normalizer).ToArray();

            return probs;
        }

        /// <summary>
        /// Takes a probability distribution of words, and samples from it
        /// </summary>
        public static (float, string) Sample(List<(float, string)> wordProbs, float totalProb = 1f)
        {
            float cummProb = 0f;
            float rng = UnityEngine.Random.Range(0, totalProb);

            for (int i = 0; i < wordProbs.Count; i++)
            {
                (float prob, string word) = wordProbs[i];
                cummProb += prob;

                if (rng <= cummProb)
                {
                    wordProbs.RemoveAt(i); // TODO: Validate
                    return (prob, word);
                }
            }

            throw new Exception("Probability Distribution Not Normalized");
        }

        /// <summary>
        /// Takes a probability distribution of words, and samples from it
        /// </summary>
        public static (float, int) Sample(List<(float, int)> wordProbs, float totalProb = 1f)
        {
            float cummProb = 0f;
            float rng = UnityEngine.Random.Range(0, totalProb);

            for (int i = 0; i < wordProbs.Count; i++)
            {
                (float prob, int idx) = wordProbs[i];
                cummProb += prob;

                if (rng <= cummProb)
                {
                    wordProbs.RemoveAt(i); // TODO: Validate
                    return (prob, idx);
                }
            }

            throw new Exception("Probability Distribution Not Normalized");
        }

        public static int[] SampleUniform(int low, int high, int numSamples, bool withReplacement = true)
        {
            int[] samples = new int[numSamples];
            List<int> indexes = Enumerable.Range(low, high - low).ToList();

            for (int i = 0; i < numSamples; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, indexes.Count());
                samples[i] = indexes[randomIndex];
                if (!withReplacement)
                    indexes.RemoveAt(randomIndex);
            }

            return samples;
        }
    }
}