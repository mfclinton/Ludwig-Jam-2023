/// Code from https://github.com/edirgarcia/bpe_sharp (MIT License)

using System;

namespace AICore
{
    public abstract class Tokenizer
    {
        public abstract Int64[] Encode(string str, bool addStartEndTokens = false);

        public abstract string Decode(Int64[] enc, bool addStartEndTokens = false);

        public abstract void Initialize(string configPath);
    }
}
