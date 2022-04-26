using System;

namespace HashidsNet.Alphabets.Salts
{
    public abstract class BaseCharsSalt : ISalt
    {
        public void Calculate(Span<int> buffer, ref int saltSum)
        {
            if (buffer.Length == 0)
                return;

            if (buffer.Length > Length)
                throw new InvalidOperationException($"The salt could calculate only {Length} values.");

            ReadOnlySpan<char> chars = Chars;

            for (var i = 0; i < buffer.Length; i++)
            {
                int x = chars[i];

                saltSum += x;

                buffer[i] = saltSum + x + i;
            }
        }

        public abstract int Length { get; }

        protected abstract char[] Chars { get; }
    }
}