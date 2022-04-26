using HashidsNet.Alphabets.Salts;

namespace HashidsNet.Alphabets
{
    public partial class LotteryAlphabet
    {
        private sealed class LotterySalt : BaseCharSalt
        {
            private readonly LotteryAlphabet _parent;

            public LotterySalt(LotteryAlphabet parent)
            {
                _parent = parent;
            }

            protected override char Char => _parent._lottery;
        }
    }
}
