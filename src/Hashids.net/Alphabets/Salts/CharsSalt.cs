using System;

namespace HashidsNet.Alphabets.Salts
{
    public sealed class CharsSalt : ISalt
    {
        private readonly char[] _chars;

        public CharsSalt(char[] chars)
        {
            _chars = chars;
        }

        public void Calculate(Span<int> buffer, int saltIndex, ref int saltSum)
        {
            if (buffer.Length == 0)
                return;

            if (buffer.Length > _chars.Length)
                throw new InvalidOperationException($"The salt could calculate only { _chars.Length} values.");

            for (var i = 0; i < buffer.Length; i++)
            {
                int x = _chars[i];

                saltSum += x;

                buffer[i] = saltSum + saltIndex + x + i;
            }
        }

        public int Length => _chars.Length;
    }
}
