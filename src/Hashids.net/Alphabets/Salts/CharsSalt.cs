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

        public void Calculate(int[] buffer, int bufferIndex, int saltIndex, int saltLength, ref int saltSum)
        {
            if (saltLength <= 0)
                return;

            if (saltLength > _chars.Length)
                throw new InvalidOperationException($"The salt could calculate only { _chars.Length} values.");

            for (var i = 0; i < _chars.Length && i < saltLength; i++)
            {
                int x = _chars[i];

                saltSum += x;

                buffer[bufferIndex + i] = saltSum + saltIndex + x + i;
            }
        }

        public int Length => _chars.Length;
    }
}
