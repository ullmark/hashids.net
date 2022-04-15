using System;

namespace HashidsNet.Alphabets
{
    public class StepAlphabet : IAlphabet
    {
        private readonly IAlphabet _inner;

        public StepAlphabet(IAlphabet inner)
        {
            _inner = inner;
        }

        public char GetChar(int index)
        {
            return _inner.GetChar(index);
        }

        public int GetIndex(char @char)
        {
            return _inner.GetIndex(@char);
        }

        public void CopyTo(Span<char> buffer, int index)
        {
            _inner.CopyTo(buffer, index);
        }

        public virtual IAlphabet NextPage()
        {
            return _inner.Clone().NextPage();
        }

        public virtual IAlphabet NextShuffle()
        {
            return _inner.Clone().NextShuffle();
        }

        public virtual IAlphabet Clone()
        {
            return new StepAlphabet(_inner);
        }

        public int Length => _inner.Length;
    }
}