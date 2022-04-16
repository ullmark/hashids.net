using HashidsNet.Alphabets.Salts;
using System;

namespace HashidsNet.Alphabets
{
    public class StepsAlphabet : AlphabetDecorator
    {
        private readonly char _lottery;
        private readonly ISalt _salt;
        private readonly int _stepsCount;

        private IAlphabet _nextPage;
        private IAlphabet _nextShuffle;

        public StepsAlphabet(IAlphabet inner, char lottery, ISalt salt, int stepsCount)
            : base(inner)
        {
            _lottery = lottery;
            _salt = salt;
            _stepsCount = stepsCount;
        }

        public override IAlphabet NextPage()
        {
            IAlphabet instance = _nextPage;

            if (instance != null)
                return instance;

            return NextAlphabet(ref _nextPage, x => x.NextPage());
        }

        public override IAlphabet NextShuffle()
        {
            IAlphabet instance = _nextShuffle;

            if (instance != null)
                return instance;

            return NextAlphabet(ref _nextShuffle, x => x.NextShuffle());
        }

        private IAlphabet NextAlphabet(
            ref IAlphabet field,
            Func<IAlphabet, IAlphabet> initializeInnerFunc)
        {
            char[] chars = new char[Inner.Length];

            Inner.CopyTo(chars, 0);

            ISalt salt = Salt.Create(_lottery).Concat(_salt);
            IAlphabet inner = new CharsAlphabet(salt, chars);

            initializeInnerFunc(inner);

            IAlphabet instance = _stepsCount > 0 ?
                new StepsAlphabet(inner, _lottery, _salt, _stepsCount - 1) :
                new StepAlphabet(inner, _lottery, _salt);

            field = instance;

            return instance;
        }
    }
}
