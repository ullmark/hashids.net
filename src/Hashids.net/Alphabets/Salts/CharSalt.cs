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

        public void Calculate(int[] buffer, int bufferIndex, int saltIndex, int saltLength, ref int saltSum)
        {
            if (saltLength <= 0)
                return;

            if (saltLength > 1)
                throw new InvalidOperationException("The one char salt could calculate only sinle value.");

            buffer[bufferIndex] = saltSum + saltIndex + _char + _char;
            saltSum += _char;
        }

        public int Length => 1;
    }
}