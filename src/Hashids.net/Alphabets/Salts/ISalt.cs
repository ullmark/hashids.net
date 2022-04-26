using System;

namespace HashidsNet.Alphabets.Salts
{
    public interface ISalt
    {
        void Calculate(Span<int> buffer, ref int saltSum);

        int Length { get; }
    }
}