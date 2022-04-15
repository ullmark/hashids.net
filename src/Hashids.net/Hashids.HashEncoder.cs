using HashidsNet.Alphabets;
using System;
using System.Buffers;

namespace HashidsNet
{
    public partial class Hashids
    {
        private struct HashEncoder
        {
            private const int MaxCharsPerNumber = 20;

            [ThreadStatic]
            private static char[] _bufferCache;

            private Hashids _parent;
            private int _alphabetLength;
            private long _hash;
            private char[] _buffer;
            private int _payloadIndex;
            private int _payloadLength;
            private int _idleLength;
            private int _numbersWritten;
            private long _lastNumber;
            private int _lastNumberIndex;
            private IAlphabet _alphabet;
            private int _index;

            public HashEncoder(Hashids parent)
                : this()
            {
                _parent = parent;
                _alphabetLength = parent._alphabetProvider.Default.Length;
            }

            public string Encode(ReadOnlySpan<long> numbers)
            {
                if (numbers.Length == 0)
                    return string.Empty;

                if (numbers.Length == 1)
                    return Encode(numbers[0]);

                for (int i = 0; i < numbers.Length; i++)
                {
                    if (numbers[i] < 0)
                        return string.Empty;
                }

                _buffer = RentBuffer(numbers);

                try
                {
                    Write(numbers, numbers.Length);

                    return BuildString();
                }
                finally
                {
                    ReturnBuffer();
                }
            }

            public string Encode(long number)
            {
                if (number < 0)
                    return string.Empty;

                InitializeBuffer();
                Prepare(number, 0);
                Write(number);
                WriteIdle();

                return BuildString();
            }

            public bool Validate(string input, long[] numbers, int numbersCount)
            {
                if (numbersCount == 0)
                    return false;

                if (numbersCount == 1)
                    return Validate(input, numbers[0]);

                for (int i = 0; i < numbersCount; i++)
                {
                    if (numbers[i] < 0)
                        return false;
                }

                _buffer = RentBuffer(numbers);

                try
                {
                    Write(numbers, numbersCount);

                    return Compare(input);
                }
                finally
                {
                    ReturnBuffer();
                }
            }

            public bool Validate(string input, long number)
            {
                if (number < 0)
                    return false;

                InitializeBuffer();
                Prepare(number, 0);
                Write(number);
                WriteIdle();

                return Compare(input);
            }

            private int GetBufferSize(int numbersCount)
            {
                return Math.Max(MaxCharsPerNumber * numbersCount, _parent._minHashLength);
            }

            private void InitializeBuffer()
            {
                int bufferSize = GetBufferSize(1);

                _buffer = _bufferCache;

                if (_buffer == null || _buffer.Length < bufferSize)
                {
                    _buffer = new char[bufferSize];
                    _bufferCache = _buffer;
                }
            }

            private char[] RentBuffer(ReadOnlySpan<long> numbers)
            {
                return ArrayPool<char>.Shared.Rent(GetBufferSize(numbers.Length));
            }

            private readonly void ReturnBuffer()
            {
                ArrayPool<char>.Shared.Return(_buffer);
            }

            private void Write(ReadOnlySpan<long> numbers, int numbersCount)
            {
                for (int i = 0; i < numbersCount; i++)
                    Prepare(numbers[i], i);

                Write(numbers[0]);

                for (int i = 1; i < numbersCount; i++)
                    Write(numbers[i]);

                WriteIdle();
            }

            private void Write(long number)
            {
                if (_numbersWritten == 0)
                {
                    CalculateIndexes();
                    WriteHeader();
                }
                else
                    WriteSeparator();

                _alphabet = _alphabet.NextPage();
                _lastNumber = number;
                _lastNumberIndex = _index;

                if (number < _alphabetLength)
                    Write((int)number);
                else
                    WriteLong(number);

                _numbersWritten++;
            }

            private void WriteLong(long number)
            {
                int length = GetLength(number);

                for (int i = _index + length - 1; i >= _index; i--)
                {
                    int charIndex = (int)(number % _alphabetLength);

                    _buffer[i] = _alphabet.GetChar(charIndex);

                    number /= _alphabetLength;
                }

                _index += length;
            }

            private void WriteHeader()
            {
                int lottery = (int)(_hash % _alphabetLength);

                _alphabet = _parent._alphabetProvider.GetAlphabet(lottery);

                Write(lottery);
            }

            private void WriteSeparator()
            {
                int sepsIndex = (int)(_lastNumber % (_buffer[_lastNumberIndex] + _numbersWritten - 1) % _parent._seps.Length);

                Write(_parent._seps[sepsIndex]);
            }

            private void WriteIdle()
            {
                if (_idleLength == 0)
                    return;

                WriteGuard(_payloadIndex - 1, _buffer[_payloadIndex]);

                if (_idleLength <= 1)
                    return;

                WriteGuard(_index, _buffer[_payloadIndex + 1]);

                if (_idleLength <= 2)
                    return;

                int leftIndex = _payloadIndex - 1;
                int rightIndex = _index + 1;
                int length = _idleLength - 2;

                while (length > 0)
                {
                    int len = Math.Min(length, _alphabetLength);
                    int leftLen = len - len / 2;
                    int rightLen = len - leftLen;

                    leftIndex -= leftLen;

                    _alphabet = _alphabet.NextShuffle();
                    _alphabet.CopyChars(_buffer, _alphabetLength - leftLen, leftIndex, leftLen);
                    _alphabet.CopyChars(_buffer, 0, rightIndex, rightLen);

                    rightIndex += rightLen;

                    length -= len;
                }
            }

            private void WriteGuard(int position, char salt)
            {
                long index = (_hash + salt) % _parent._guards.Length;

                _buffer[position] = _parent._guards[index];
            }

            private void Write(int charIndex)
            {
                Write(_alphabet.GetChar(charIndex));
            }

            private void Write(char @char)
            {
                _buffer[_index++] = @char;
            }

            private void Prepare(long number, int position)
            {
                _hash += number % (position + 100);
                _payloadLength += GetLength(number) + 1;
            }

            private void CalculateIndexes()
            {
                if (_parent._minHashLength < _payloadLength)
                    return;

                _idleLength = _parent._minHashLength - _payloadLength;
                _payloadIndex = _idleLength - _idleLength / 2;
                _index = _payloadIndex;
            }

            private int GetLength(long number)
            {
                if (number <= _alphabetLength)
                    return 1;

                return (int)Math.Log(number, _alphabetLength) + 1;
            }

            private string BuildString()
            {
                return new string(_buffer, 0, _idleLength + _payloadLength);
            }

            private bool Compare(string input)
            {
                if (input.Length != _idleLength + _payloadLength)
                    return false;

                for (int i = 0; i < input.Length; i++)
                {
                    if (input[i] != _buffer[i])
                        return false;
                }

                return true;
            }
        }
    }
}
