using HashidsNet.Alphabets.Salts;
using System;

namespace HashidsNet.Alphabets
{
    public class AlphabetProvider : IAlphabetProvider
    {
        private readonly char[] _chars;
        private readonly ISalt _salt;
        private readonly CharsAlphabet _default;

        public AlphabetProvider(char[] chars, char[] salt)
        {
            _chars = chars;
            _salt = Salt.Create(salt).Snapshot();
            _default = new CharsAlphabet(_salt, chars);
        }

        public virtual IAlphabet GetAlphabet(int index)
        {
            return LotteryAlphabet.Get(_default, _default.GetChar(index), _salt);
        }

        public IAlphabet GetAlphabet(char @char)
        {
            int index = Array.IndexOf(_chars, @char);

            return index >= 0 ? GetAlphabet(index) : null;
        }

        public IAlphabet Default => _default;
    }
}
