using HashidsNet.Alphabets;
using System;

namespace HashidsNet
{
    public partial class Hashids
    {
        private ref struct EncodingContext
        {
            public Hashids Parent;
            public HashStats Stats;
            public Span<char> Buffer;
            public IAlphabet Alphabet;
            public int Index;

            public EncodingContext(Hashids parent, HashStats stats, Span<char> buffer)
            {
                Parent = parent;
                Stats = stats;
                Buffer = buffer;

                Alphabet = parent._alphabetProvider.GetAlphabet(stats.Lottery);
                Index = 0;
            }
        }
    }
}
