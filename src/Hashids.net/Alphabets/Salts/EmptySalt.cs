using System;

namespace HashidsNet.Alphabets.Salts
{
    public class EmptySalt : ISalt
    {
        public static readonly EmptySalt Instance = new EmptySalt();

        private EmptySalt()
        {
        }

        public void Calculate(int[] buffer, int bufferIndex, int saltIndex, int saltLength, ref int saltSum)
        {
            if (saltLength > 0)
                throw new InvalidOperationException();
        }

        public int Length => 0;
    }
}
