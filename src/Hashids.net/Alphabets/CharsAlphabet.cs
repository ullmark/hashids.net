using HashidsNet.Alphabets.Salts;
using System;

namespace HashidsNet.Alphabets
{
    public sealed class CharsAlphabet : IAlphabet
    {
        private readonly ISalt _salt;
        private readonly char[] _chars;

        public CharsAlphabet(ISalt salt, char[] chars)
        {
            _salt = salt;
            _chars = new char[chars.Length];

            Array.Copy(chars, 0, _chars, 0, chars.Length);
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
            _salt.Concat(_chars).Shuffle(_chars);

            return this;
        }

        public IAlphabet NextShuffle()
        {
            Salt.Create(_chars).Shuffle(_chars);

            return this;
        }

        public IAlphabet Clone()
        {
            return new CharsAlphabet(_salt, _chars);
        }

        public int Length => _chars.Length;
    }
}
