using System;

namespace HashidsNet.Alphabets.Salts
{
    public class ValueSalt : ISalt
    {
        private int[] _value;
        private int _sum;

        public ValueSalt(int[] value, int sum)
        {
            _value = value;
            _sum = sum;
        }

        public void Calculate(int[] buffer, int bufferIndex, int saltIndex, int saltLength, ref int saltSum)
        {
            Array.Copy(_value, 0, buffer, bufferIndex, saltLength);

            int last = bufferIndex + saltLength;

            for (var i = bufferIndex; i < last; i++)
                buffer[i] += saltIndex + saltSum;

            saltSum += _sum;
        }

        public int Length => _value.Length;
    }
}