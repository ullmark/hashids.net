using System;

namespace HashidsNet.Alphabets.Salts
{
    public abstract class BaseCharSalt : ISalt
    {
        public void Calculate(Span<int> buffer, ref int saltSum)
        {
            if (buffer.Length == 0)
                return;

            if (buffer.Length > 1)
                throw new InvalidOperationException("The one char salt could calculate only sinle value.");

            buffer[0] = saltSum + Char + Char;
            saltSum += Char;
        }

        public int Length => 1;

        protected abstract char Char { get; }
    }
}