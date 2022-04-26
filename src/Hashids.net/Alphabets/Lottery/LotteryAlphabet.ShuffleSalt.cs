using HashidsNet.Alphabets.Salts;

namespace HashidsNet.Alphabets.Lottery
{
    public partial class LotteryAlphabet
    {
        private sealed class ShuffleSalt : BaseCharsSalt
        {
            private readonly LotteryAlphabet _parent;

            public ShuffleSalt(LotteryAlphabet parent)
            {
                _parent = parent;
            }

            public override int Length => _parent.Length;

            protected override char[] Chars => _parent._chars;
        }
    }
}
