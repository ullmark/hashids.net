using HashidsNet.Alphabets.Salts;
using System;

namespace HashidsNet.Alphabets
{
    public sealed class CharsAlphabet : IAlphabet
    {
        private readonly ISalt _shuffleSalt;
        private readonly ISalt _pageSalt;
        private readonly char[] _chars;

        public CharsAlphabet(ISalt salt, char[] chars)
        {
            _shuffleSalt = Salt.Create(chars);
            _pageSalt = salt.Concat(_shuffleSalt);
            _chars = chars;
        }

        public char GetChar(int index)
        {
            return _chars[index];
        }

        public int GetIndex(char @char)
        {
            return Array.IndexOf(_chars, @char);
        }

        public void CopyTo(Span<char> buffer, int index)
        {
            _chars.AsSpan(index, buffer.Length).CopyTo(buffer);
        }

        public IAlphabet NextPage()
        {
            _pageSalt.Shuffle(_chars);

            return this;
        }

        public IAlphabet NextShuffle()
        {
            _shuffleSalt.Shuffle(_chars);

            return this;
        }

        public IAlphabet Return()
        {
            return this;
        }

        public IAlphabet Clone()
        {
            return new CharsAlphabet(_pageSalt, _chars);
        }

        public int Length => _chars.Length;
    }
}
