using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace AICore
{
    /// GPT2 Python Tokenizer:
    /// https://github.com/huggingface/transformers/blob/main/src/transformers/models/gpt2/tokenization_gpt2.py
    /// https://huggingface.co/course/chapter6/5?fw=pt
    public class GPT2Tokenizer : Tokenizer
    {
        Tokenizer tokenizer;

        public GPT2Tokenizer()
        {
            var tokenizerDataFile = Application.dataPath + @"/StreamingAssets/AIModels/LLMs/GPTDecoders/GPT2OpenAI/tokenizer.json";
            Initialize(tokenizerDataFile);
        }

        public override void Initialize(string configPath)
        {
            tokenizer = new BPETokenizer(configPath);
        }

        /// Converts a string to a sequence of ids (integer)
        public override Int64[] Encode(string text, bool addStartEndTokens = false)
        {
            var encoded = tokenizer.Encode(text, addStartEndTokens);
            return encoded;
        }

        // Convert a list of lists of token ids into a list of strings by calling decode.
        public override string Decode(Int64[] encodedSeq, bool addStartEndTokens = false)
        {
            var decoded = tokenizer.Decode(encodedSeq, addStartEndTokens);
            return decoded;
        }
    }
}