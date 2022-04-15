using System;

namespace HashidsNet.Alphabets.Salts
{
    public class ValueSalt : ISalt
    {
        private readonly int[] _value;
        private readonly int _sum;

        public ValueSalt(int[] value, int sum)
        {
            _value = value;
            _sum = sum;
        }

        public void Calculate(Span<int> buffer, int saltIndex, ref int saltSum)
        {
            for (var i = 0; i < buffer.Length; i++)
                buffer[i] = _value[i] + saltIndex + saltSum;

            saltSum += _sum;
        }

        public int Length => _value.Length;
    }
}