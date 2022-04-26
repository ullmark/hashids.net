using HashidsNet.Alphabets.Salts;
using System;

namespace HashidsNet.Alphabets
{
    public partial class LotteryAlphabet
    {
        private sealed class PageSalt : ISalt
        {
            private readonly LotteryAlphabet _parent;
            private readonly LotterySalt _lotterySalt;

            public PageSalt(LotteryAlphabet parent)
            {
                _parent = parent;
                _lotterySalt = new LotterySalt(_parent);
            }

            public void Calculate(Span<int> buffer, ref int saltSum)
            {
                if (buffer.Length == 0)
                    return;

                int index = 0;

                ConcatSalt.Calculate(_lotterySalt, buffer, ref index, ref saltSum);
                ConcatSalt.Calculate(_parent._salt, buffer, ref index, ref saltSum);
                ConcatSalt.Calculate(_parent._shuffleSalt, buffer, ref index, ref saltSum);
            }

            public int Length => _parent._salt.Length + _parent._shuffleSalt.Length + 1;
        }
    }
}
