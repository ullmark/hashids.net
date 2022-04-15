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
            _salt = Salt.Create(salt).TakeSnapshot();
            _default = new CharsAlphabet(_salt, chars);
        }

        public virtual IAlphabet GetAlphabet(int index)
        {
            ISalt salt = Salt
                 .Create(_default.GetChar(index))
                 .Concat(_salt);

            return new CharsAlphabet(salt, _chars);
        }

        public IAlphabet GetAlphabet(char @char)
        {
            int index = Array.IndexOf(_chars, @char);

            return index >= 0 ? GetAlphabet(index) : null;
        }

        public IAlphabet Default => _default;
    }
}
