using HashidsNet.Alphabets;
using System;
using System.Buffers;

namespace HashidsNet
{
    public partial class Hashids
    {
        private ref struct HashEncoder
        {
            private const int MaxCharsPerNumber = 20;

            [ThreadStatic]
            private static char[] _bufferCache;

            private readonly Hashids _parent;
            private readonly Span<char> _buffer;
            private IAlphabet _alphabet;
            private long _payloadHash;
            private int _payloadIndex;
            private int _payloadLength;
            private int _idleLength;
            private int _numbersCount;
            private long _lastNumber;
            private int _lastNumberIndex;
            private int _index;

            public HashEncoder(Hashids parent, Span<char> buffer)
                : this()
            {
                _parent = parent;
                _buffer = buffer;
                _alphabet = _parent._alphabetProvider.Default;
            }

            public static string Encode(Hashids parent, ReadOnlySpan<long> numbers)
            {
                if (numbers.Length == 0)
                    return string.Empty;

                if (numbers.Length == 1)
                    return Encode(parent, numbers[0]);

                for (int i = 0; i < numbers.Length; i++)
                {
                    if (numbers[i] < 0)
                        return string.Empty;
                }

                Span<char> buffer = stackalloc char[GetBufferSize(parent, numbers.Length)];
                HashEncoder encoder = new HashEncoder(parent, buffer);

                encoder.Write(numbers);

                return encoder.BuildString();
            }

            public static string Encode(Hashids parent, long number)
            {
                if (number < 0)
                    return string.Empty;

                Span<char> buffer = stackalloc char[GetBufferSize(parent, 1)];
                HashEncoder encoder = new HashEncoder(parent, buffer);

                encoder.Prepare(number, 0);
                encoder.Write(number);
                encoder.WriteIdle();

                return encoder.BuildString();
            }

            public static bool Validate(Hashids parent, string input, ReadOnlySpan<long> numbers)
            {
                if (numbers.Length == 0)
                    return false;

                if (numbers.Length == 1)
                    return Validate(parent, input, numbers[0]);

                for (int i = 0; i < numbers.Length; i++)
                {
                    if (numbers[i] < 0)
                        return false;
                }

                Span<char> buffer = stackalloc char[GetBufferSize(parent, numbers.Length)];
                HashEncoder encoder = new HashEncoder(parent, buffer);

                encoder.Write(numbers);

                return encoder.Compare(input);
            }

            public static bool Validate(Hashids parent, string input, long number)
            {
                if (number < 0)
                    return false;

                Span<char> buffer = stackalloc char[GetBufferSize(parent, 1)];
                HashEncoder encoder = new HashEncoder(parent, buffer);

                encoder.Prepare(number, 0);
                encoder.Write(number);
                encoder.WriteIdle();

                return encoder.Compare(input);
            }

            private static int GetBufferSize(Hashids parent, int numbersCount)
            {
                return Math.Max(MaxCharsPerNumber * numbersCount, parent._minHashLength);
            }

            private void Write(ReadOnlySpan<long> numbers)
            {
                for (int i = 0; i < numbers.Length; i++)
                    Prepare(numbers[i], i);

                Write(numbers[0]);

                for (int i = 1; i < numbers.Length; i++)
                    Write(numbers[i]);

                WriteIdle();
            }

            private void Write(long number)
            {
                if (_numbersCount == 0)
                {
                    CalculateIndexes();
                    WriteHeader();
                }
                else
                    WriteSeparator();

                _alphabet = _alphabet.NextPage();
                _lastNumber = number;
                _lastNumberIndex = _index;

                if (number < _alphabet.Length)
                    Write((int)number);
                else
                    WriteLong(number);

                _numbersCount++;
            }

            private void WriteLong(long number)
            {
                int length = GetLength(number);

                for (int i = _index + length - 1; i >= _index; i--)
                {
                    int charIndex = (int)(number % _alphabet.Length);

                    _buffer[i] = _alphabet.GetChar(charIndex);

                    number /= _alphabet.Length;
                }

                _index += length;
            }

            private void WriteHeader()
            {
                int lottery = (int)(_payloadHash % _alphabet.Length);

                _alphabet = _parent._alphabetProvider.GetAlphabet(lottery);

                Write(lottery);
            }

            private void WriteSeparator()
            {
                int sepsIndex = (int)(_lastNumber % (_buffer[_lastNumberIndex] + _numbersCount - 1) % _parent._seps.Length);

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
                    int len = Math.Min(length, _alphabet.Length);
                    int leftLen = len - len / 2;
                    int rightLen = len - leftLen;

                    leftIndex -= leftLen;

                    _alphabet = _alphabet.NextShuffle();

                    Span<char> leftSpan = _buffer.Slice(leftIndex, leftLen);
                    Span<char> rightSpan = _buffer.Slice(rightIndex, rightLen);

                    _alphabet.CopyTo(leftSpan, _alphabet.Length - leftLen);
                    _alphabet.CopyTo(rightSpan, 0);

                    rightIndex += rightLen;

                    length -= len;
                }
            }

            private void WriteGuard(int position, char salt)
            {
                long index = (_payloadHash + salt) % _parent._guards.Length;

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
                _payloadHash += number % (position + 100);
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
                if (number < _alphabet.Length)
                    return 1;

                return (int)Math.Log(number, _alphabet.Length) + 1;
            }

            private string BuildString()
            {
                return _buffer.Slice(0, _idleLength + _payloadLength).ToString();
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
