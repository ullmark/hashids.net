using HashidsNet.Alphabets.Salts;
using System;

namespace HashidsNet.Alphabets
{
    public class AlphabetProvider : IAlphabetProvider
    {
        public AlphabetProvider(char[] chars, char[] salt)
        {
            Chars = chars;
            Salt = Salts.Salt.Create(salt).Snapshot();
            Default = new CharsAlphabet(Salt, chars);
        }

        public virtual IAlphabet GetAlphabet(int index)
        {
            return LotteryAlphabet.Get(Default, Default.GetChar(index), Salt);
        }

        public IAlphabet GetAlphabet(char @char)
        {
            int index = Array.IndexOf(Chars, @char);

            return index >= 0 ? GetAlphabet(index) : null;
        }

        public IAlphabet Default { get; }

        protected char[] Chars { get; }
        protected ISalt Salt { get; }
    }
}
