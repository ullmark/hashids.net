using HashidsNet.Alphabets.Lottery;
using HashidsNet.Alphabets.Salts;

namespace HashidsNet.Alphabets
{
    public class StepAlphabet : AlphabetDecorator, IAlphabet
    {
        private readonly char _lottery;
        private readonly ISalt _salt;

        public StepAlphabet(IAlphabet inner, char lottery, ISalt salt)
            : base(inner)
        {
            _lottery = lottery;
            _salt = salt;
        }

        public override IAlphabet NextPage()
        {
            return LotteryAlphabet.Get(Inner, _lottery, _salt).NextPage();
        }

        public override IAlphabet NextShuffle()
        {
            return LotteryAlphabet.Get(Inner, _lottery, _salt).NextShuffle();
        }

        public override IAlphabet Return()
        {
            return this;
        }
    }
}