using System;

namespace HashidsNet.Alphabets
{
    public abstract class AlphabetDecorator : IAlphabet
    {
        public AlphabetDecorator(IAlphabet inner)
        {
            Inner = inner;
        }

        public void CopyTo(Span<char> buffer, int index)
        {
            Inner.CopyTo(buffer, index);
        }

        public char GetChar(int index)
        {
            return Inner.GetChar(index);
        }

        public int GetIndex(char @char)
        {
            return Inner.GetIndex(@char);
        }

        public virtual IAlphabet NextPage()
        {
            return Inner.NextPage();
        }

        public virtual IAlphabet NextShuffle()
        {
            return Inner.NextShuffle();
        }

        public virtual IAlphabet Return()
        {
            return Inner.Return();
        }

        public IAlphabet Inner { get; }

        public int Length => Inner.Length;
    }
}