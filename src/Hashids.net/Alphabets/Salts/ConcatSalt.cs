using System;

namespace HashidsNet.Alphabets.Salts
{
    public class ConcatSalt : ISalt
    {
        private readonly ISalt _x;
        private readonly ISalt _y;

        public ConcatSalt(ISalt x, ISalt y)
        {
            _x = x;
            _y = y;
        }

        public static void Calculate(ISalt salt, Span<int> buffer, ref int saltIndex, ref int saltSum)
        {
            int length = Math.Min(salt.Length, buffer.Length - saltIndex);

            if (length <= 0)
                return;

            buffer = buffer.Slice(saltIndex, length);

            salt.Calculate(buffer, ref saltSum);

            if (saltIndex > 0)
            {
                for (int i = 0; i < length; i++)
                    buffer[i] += saltIndex;
            }

            saltIndex += length;
        }

        public void Calculate(Span<int> buffer, ref int saltSum)
        {
            if (buffer.Length == 0)
                return;

            int index = 0;

            Calculate(_x, buffer, ref index, ref saltSum);
            Calculate(_y, buffer, ref index, ref saltSum);
        }

        public int Length => _x.Length + _y.Length;
    }
}