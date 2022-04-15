using System;

namespace HashidsNet.Alphabets
{
    public class StepsAlphabet : StepAlphabet
    {
        private readonly int _stepsCount;

        private IAlphabet _nextPage;
        private IAlphabet _nextShuffle;

        public StepsAlphabet(IAlphabet inner, int stepsCount)
            : base(inner)
        {
            _stepsCount = stepsCount;
        }

        public override IAlphabet NextPage()
        {
            IAlphabet instance = _nextPage;

            if (instance != null)
                return instance;

            return NextAlphabet(ref _nextPage, () => base.NextPage());
        }

        public override IAlphabet NextShuffle()
        {
            IAlphabet instance = _nextShuffle;

            if (instance != null)
                return instance;

            return NextAlphabet(ref _nextShuffle, () => base.NextShuffle());
        }

        public override IAlphabet Clone()
        {
            return (IAlphabet)MemberwiseClone();
        }

        private IAlphabet NextAlphabet(
            ref IAlphabet field,
            Func<IAlphabet> initializeInnerFunc)
        {
            IAlphabet inner = initializeInnerFunc();

            IAlphabet instance = _stepsCount > 0 ?
                new StepsAlphabet(inner, _stepsCount - 1) :
                new StepAlphabet(inner);

            field = instance;

            return instance;
        }
    }
}
