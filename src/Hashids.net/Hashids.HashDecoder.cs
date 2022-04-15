using HashidsNet.Alphabets;
using System;
using System.Linq;

namespace HashidsNet
{
    public partial class Hashids
    {
        private struct HashDecoder
        {
            [ThreadStatic]
            private static long[] _bufferCache;

            private readonly Hashids _parent;
            private IAlphabet _alphabet;
            private string _input;
            private char _char;
            private int _index;
            private long[] _buffer;
            private int _numbersCount;

            public HashDecoder(Hashids parent)
                : this()
            {
                _parent = parent;
            }

            public long[] Decode(string input)
            {
                if (string.IsNullOrEmpty(input))
                    return Array.Empty<long>();

                _input = input;

                ReadIdle();

                if (!Read())
                    return Array.Empty<long>();

                _alphabet = _parent._alphabetProvider.GetAlphabet(_char);

                if (_alphabet == null)
                    return Array.Empty<long>();

                InitializeBuffer();
                ReadNumbers();

                return Validate() ? BuildArray() : Array.Empty<long>();
            }

            private void InitializeBuffer()
            {
                int length = (_input.Length - (_index - 1) * 2) / 2;

                _buffer = _bufferCache;

                if (_buffer == null || _buffer.Length < length)
                {
                    _buffer = new long[length];
                    _bufferCache = _buffer;
                }
            }

            private void ReadIdle()
            {
                while (Read())
                {
                    if (IsGuard())
                        return;
                }

                _index = 0;
            }

            private bool Read()
            {
                if (_index >= _input.Length)
                {
                    _char = default(char);
                    return false;
                }

                _char = _input[_index++];
                return true;
            }

            private void ReadNumbers()
            {
                long? number = null;

                while (Read())
                {
                    if (IsGuard())
                        break;

                    if (IsSeparator())
                    {
                        Push(number);
                        number = null;
                        continue;
                    }

                    if (number == null)
                    {
                        number = 0L;
                        _alphabet = _alphabet.NextPage();
                    }

                    number *= _alphabet.Length;
                    number += _alphabet.GetIndex(_char);
                }

                Push(number);
            }

            private void Push(long? number)
            {
                if (number == null)
                    return;

                if (_numbersCount >= _buffer.Length)
                    throw new InvalidOperationException("There is no room for a number.");

                _buffer[_numbersCount++] = number.GetValueOrDefault();
            }

            private bool IsGuard()
            {
                return _parent._guards.Contains(_char);
            }

            private bool IsSeparator()
            {
                return _parent._seps.Contains(_char);
            }

            private bool Validate()
            {
                return _parent.NewEncoder().Validate(_input, _buffer, _numbersCount);
            }

            private long[] BuildArray()
            {
                long[] numbers = new long[_numbersCount];

                Array.Copy(_buffer, numbers, _numbersCount);

                return numbers;
            }
        }
    }
}
