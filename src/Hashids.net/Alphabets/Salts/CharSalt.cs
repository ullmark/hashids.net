using System;

namespace HashidsNet.Alphabets.Salts
{
    public class CharSalt : ISalt
    {
        private readonly char _char;

        public CharSalt(char @char)
        {
            _char = @char;
        }

        public void Calculate(Span<int> buffer, int saltIndex, ref int saltSum)
        {
            if (buffer.Length == 0)
                return;

            if (buffer.Length > 1)
                throw new InvalidOperationException("The one char salt could calculate only sinle value.");

            buffer[0] = saltSum + saltIndex + _char + _char;
            saltSum += _char;
        }

        public int Length => 1;
    }
}