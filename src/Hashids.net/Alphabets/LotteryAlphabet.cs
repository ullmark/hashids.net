using HashidsNet.Alphabets.Salts;
using System;
using System.Threading;

namespace HashidsNet.Alphabets
{
    public class LotteryAlphabet : IAlphabet
    {
        [ThreadStatic]
        private static LotteryAlphabet _cache;

        private readonly char[] _chars;
        private readonly CharSalt _lotterySalt;
        private readonly WeekSalt _salt;
        private readonly CharsSalt _shuffleSalt;
        private readonly ConcatSalt _pageSalt;

        private LotteryAlphabet(int maxlength)
        {
            _chars = new char[maxlength];

            _lotterySalt = new CharSalt();
            _salt = new WeekSalt();

            _shuffleSalt = new CharsSalt(_chars);
            _shuffleSalt.Length = 0;

            _pageSalt =
                new ConcatSalt(
                    new ConcatSalt(_lotterySalt, _salt),
                    _shuffleSalt);

            Length = 0;
        }

        public static LotteryAlphabet Get(IAlphabet baseAlphabet, char lottery, ISalt salt)
        {
            LotteryAlphabet alphabet = _cache;

            if (alphabet == null || alphabet.Length < baseAlphabet.Length)
                alphabet = new LotteryAlphabet(baseAlphabet.Length);

            Span<char> span = alphabet._chars.AsSpan(0, baseAlphabet.Length);
            baseAlphabet.CopyTo(span, 0);

            alphabet._lotterySalt.Char = lottery;
            alphabet._salt.Inner = salt;
            alphabet._shuffleSalt.Length = baseAlphabet.Length;
            alphabet.Length = baseAlphabet.Length;

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

        public int Length { get; private set; }
    }
}
