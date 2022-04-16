using System;

namespace HashidsNet.Alphabets.Salts
{
    public class WeekSalt : ISalt
    {
        public void Calculate(Span<int> buffer, int saltIndex, ref int saltSum)
        {
            if (buffer.Length == 0)
                return;

            if (Inner == null)
                throw new InvalidOperationException("The salt should be specified.");

            Inner.Calculate(buffer, saltIndex, ref saltSum);
        }

        public ISalt Inner { get; set; }

        public int Length => Inner?.Length ?? 0;
    }
}
