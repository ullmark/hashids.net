namespace HashidsNet.Alphabets
{
    public interface IAlphabetProvider
    {
        IAlphabet GetAlphabet(int index);
        IAlphabet GetAlphabet(char @char);
        IAlphabet Default { get; }
    }
}