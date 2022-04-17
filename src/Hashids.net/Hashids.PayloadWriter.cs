using HashidsNet.Alphabets;
using System;

namespace HashidsNet
{
    public partial class Hashids
    {
        private ref struct PayloadWriter
        {
            private readonly Hashids _parent;
            private readonly HashStats _stats;
            private readonly Span<char> _buffer;

            private IAlphabet _alphabet;
            private int _index;
            private int _numbersWritten;
            private long _lastNumber;
            private char _lastNumberChar;

            private PayloadWriter(EncodingContext context)
            {
                _parent = context.Parent;
                _stats = context.Stats;
                _buffer = context.Buffer;
                _alphabet = context.Alphabet;
                _index = _stats.PayloadIndex;
                _numbersWritten = 0;
                _lastNumber = 0L;
                _lastNumberChar = '\0';
            }

            public static void Write(ref EncodingContext context, long number)
            {
                PayloadWriter writer = new PayloadWriter(context);

                writer.Write(number);
                writer.Update(ref context);
            }

            public static void Write(ref EncodingContext context, ReadOnlySpan<long> numbers)
            {
                PayloadWriter writer = new PayloadWriter(context);

                for (int i = 0; i < numbers.Length; i++)
                    writer.Write(numbers[i]);

                writer.Update(ref context);
            }

            private void Write(long number)
            {
                if (_numbersWritten == 0)
                    WriteHeader();
                else
                    WriteSeparator();

                _alphabet = _alphabet.NextPage();
                _lastNumber = number;

                if (number < _stats.AlphabetLength)
                {
                    _lastNumberChar = _alphabet.GetChar((int)number);
                    Write(_lastNumberChar);
                }
                else
                    WriteLong(number);

                _numbersWritten++;
            }

            private void WriteLong(long number)
            {
                int alphabetLength = _stats.AlphabetLength;
                int payloadLength = _stats.GetPayloadLength(number);

                for (int i = _index + payloadLength - 1; i >= _index; i--)
                {
                    int charIndex = (int)(number % alphabetLength);

                    _lastNumberChar = _alphabet.GetChar(charIndex);
                    _buffer[i] = _lastNumberChar;

                    number /= alphabetLength;
                }

                _index += payloadLength;
            }

            private void WriteHeader()
            {
                Write(_stats.Lottery);
            }

            private void WriteSeparator()
            {
                char separator = _parent.GetSeparator(_lastNumber, _numbersWritten - 1, _lastNumberChar);

                Write(separator);
            }

            private void Write(int alphabetIndex)
            {
                Write(_alphabet.GetChar(alphabetIndex));
            }

            private void Write(char @char)
            {
                _buffer[_index++] = @char;
            }

            private void Update(ref EncodingContext context)
            {
                context.Alphabet = _alphabet;
                context.Index = _index;
            }
        }
    }
}
