using System;

namespace HashidsNet.Alphabets.Salts
{
    public sealed class CharsSalt : ISalt
    {
        private readonly char[] _chars;
        private int _length;

        public CharsSalt(char[] chars)
        {
            _chars = chars;

            Length = _chars.Length;
        }

        public void Calculate(Span<int> buffer, int saltIndex, ref int saltSum)
        {
            if (buffer.Length == 0)
                return;

            if (buffer.Length > _length)
                throw new InvalidOperationException($"The salt could calculate only {_length} values.");

            for (var i = 0; i < buffer.Length; i++)
            {
                int x = _chars[i];

                saltSum += x;

                buffer[i] = saltSum + saltIndex + x + i;
            }
        }

        public int Length
        {
            get { return _length; }
            set
            {
                if (value > _chars.Length)
                    throw new IndexOutOfRangeException();

                _length = value;
            }
        }
    }
}
