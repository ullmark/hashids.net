using System;

namespace HashidsNet.Alphabets
{
    public interface IAlphabet
    {
        char GetChar(int index);
        int GetIndex(char @char);

        void CopyTo(Span<char> buffer, int index);

        IAlphabet NextPage();
        IAlphabet NextShuffle();
        IAlphabet Return();

        int Length { get; }
    }
}