using System;

namespace HashidsNet.Alphabets.Salts
{
    public class CharSalt : ISalt
    {
        public void Calculate(Span<int> buffer, int saltIndex, ref int saltSum)
        {
            if (buffer.Length == 0)
                return;

            if (buffer.Length > 1)
                throw new InvalidOperationException("The one char salt could calculate only sinle value.");

            buffer[0] = saltSum + saltIndex + Char + Char;
            saltSum += Char;
        }

        public char Char { get; set; }

        public int Length => 1;
    }
}