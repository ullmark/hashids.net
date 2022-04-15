namespace HashidsNet.Alphabets
{
    public interface IAlphabet
    {
        char GetChar(int index);
        int GetIndex(char @char);
        void CopyChars(char[] buffer, int sourceIndex, int destinationIndex, int length);
        IAlphabet NextPage();
        IAlphabet NextShuffle();
        IAlphabet Clone();

        int Length { get; }

    }
}