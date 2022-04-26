using System;

namespace HashidsNet.Alphabets.Salts
{
    public class EmptySalt : ISalt
    {
        public static readonly EmptySalt Instance = new EmptySalt();

        private EmptySalt()
        {
        }

        public void Calculate(Span<int> buffer, ref int saltSum)
        {
            if (buffer.Length > 0)
                throw new InvalidOperationException();
        }

        public int Length => 0;
    }
}
