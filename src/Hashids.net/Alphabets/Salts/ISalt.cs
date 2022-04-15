namespace HashidsNet.Alphabets.Salts
{
    public interface ISalt
    {
        void Calculate(int[] buffer, int bufferIndex, int saltIndex, int saltLength, ref int saltSum);

        int Length { get; }
    }
}