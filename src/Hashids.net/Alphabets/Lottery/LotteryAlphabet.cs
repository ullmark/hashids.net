using HashidsNet.Alphabets.Salts;
using System;

namespace HashidsNet.Alphabets.Lottery
{
    public partial class LotteryAlphabet : IAlphabet
    {
        [ThreadStatic]
        private static LotteryAlphabet _cache;

        private readonly ShuffleSalt _shuffleSalt;
        private readonly PageSalt _pageSalt;

        private char[] _chars;
        private char _lottery;
        private ISalt _salt;

        private LotteryAlphabet()
        {
            _shuffleSalt = new ShuffleSalt(this);
            _pageSalt = new PageSalt(this);

            Reset();
        }

        public static LotteryAlphabet Get(IAlphabet baseAlphabet, char lottery, ISalt salt)
        {
            LotteryAlphabet alphabet = _cache ?? new LotteryAlphabet();

            alphabet.Set(baseAlphabet, lottery, salt);

            return alphabet;
        }

        public char GetChar(int index)
        {
            return _chars[index];
        }

        public int GetIndex(char @char)
        {
            return Array.IndexOf(_chars, @char, 0, Length);
        }

        public void CopyTo(Span<char> buffer, int index)
        {
            if (buffer.Length + index > Length)
                throw new ArgumentOutOfRangeException("Sum of index and buffer length should not exceed length of alphabet.");

            _chars.AsSpan(index, buffer.Length).CopyTo(buffer);
        }

        public IAlphabet NextPage()
        {
            _pageSalt.Shuffle(_chars, Length);

            return this;
        }

        public IAlphabet NextShuffle()
        {
            _shuffleSalt.Shuffle(_chars, Length);

            return this;
        }

        public IAlphabet Return()
        {
            _cache = this;

            return null;
        }

        private void Set(IAlphabet baseAlphabet, char lottery, ISalt salt)
        {
            if (_chars.Length < baseAlphabet.Length)
                _chars = new char[baseAlphabet.Length];

            Span<char> span = _chars.AsSpan(0, baseAlphabet.Length);
            baseAlphabet.CopyTo(span, 0);

            _lottery = lottery;
            _salt = salt;

            Length = baseAlphabet.Length;
        }

        private void Reset()
        {
            _chars = Array.Empty<char>();
            _lottery = '\0';
            _salt = EmptySalt.Instance;

            Length = 0;
        }

        public int Length { get; private set; }
    }
}
