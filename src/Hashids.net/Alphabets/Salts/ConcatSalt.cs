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

        public void Calculate(Span<int> buffer, int saltIndex, ref int saltSum)
        {
            if (buffer.Length == 0)
                return;

            int xLength = Math.Min(_x.Length, buffer.Length);
            int yLength = buffer.Length - xLength;

            _x.Calculate(buffer.Slice(0, xLength), saltIndex, ref saltSum);
            _y.Calculate(buffer.Slice(xLength, yLength), saltIndex + xLength, ref saltSum);
        }

        public int Length => _x.Length + _y.Length;
    }
}