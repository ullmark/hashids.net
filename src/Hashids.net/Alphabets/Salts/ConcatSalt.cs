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

        public void Calculate(int[] buffer, int bufferIndex, int saltIndex, int saltLength, ref int saltSum)
        {
            int xLength = Math.Min(_x.Length, saltLength);
            int yLength = saltLength - xLength;

            _x.Calculate(buffer, bufferIndex, saltIndex, xLength, ref saltSum);
            _y.Calculate(buffer, bufferIndex + xLength, saltIndex + xLength, yLength, ref saltSum);
        }

        public int Length => _x.Length + _y.Length;
    }
}