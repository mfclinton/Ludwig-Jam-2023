using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace AICore
{
    /// GPT2 Python Tokenizer:
    /// https://github.com/huggingface/transformers/blob/main/src/transformers/models/gpt2/tokenization_gpt2.py
    /// https://huggingface.co/course/chapter6/5?fw=pt
    public class RobertaMNLITokenizer : Tokenizer
    {
        Tokenizer tokenizer;
        string defaultTemplate;

        public RobertaMNLITokenizer(string defaultTemplate = @"This text is {0}")
        {
            var tokenizerDataFile = Application.dataPath + @"/StreamingAssets/AIModels/LLMs/Roberta/tokenizer.json";
            Initialize(tokenizerDataFile);
            this.defaultTemplate = defaultTemplate;
        }

        public override void Initialize(string configPath)
        {
            tokenizer = new BPETokenizer(configPath);
        }

        /// Converts a string to a sequence of ids (integer)
        public override Int64[] Encode(string text, bool addStartEndTokens = true)
        {
            throw new Exception("Regular Encode not supported with MNLI");
        }

        /// Converts a string to a sequence of ids (integer)
        public Int64[] Encode(string text, string label, bool addStartEndTokens = true)
        {
            return EncodePremHyp(text, string.Format(defaultTemplate, label), addStartEndTokens);
        }

        /// Converts a string to a sequence of ids (integer)
        public Int64[] EncodePremHyp(string premise, string hypothesis, bool addStartEndTokens = true)
        {
            var premiseEncoded = tokenizer.Encode(premise, addStartEndTokens);
            var hypothesisEncoded = tokenizer.Encode(hypothesis, addStartEndTokens);
            hypothesisEncoded[0] = 2;

            var encoded = new long[premiseEncoded.Length + hypothesisEncoded.Length];
            premiseEncoded.CopyTo(encoded, 0);
            hypothesisEncoded.CopyTo(encoded, premiseEncoded.Length);

            return encoded;
        }

        // Convert a list of lists of token ids into a list of strings by calling decode.
        public override string Decode(Int64[] encodedSeq, bool addStartEndTokens = true)
        {
            throw new Exception("Decode not supported for MNLI");
        }
    }
}