using HashidsNet.Alphabets;
using System;
using System.Linq;

namespace HashidsNet
{
    public partial class Hashids
    {
        private ref struct HashDecoder
        {
            private readonly Hashids _parent;
            private readonly Span<long> _buffer;
            private IAlphabet _alphabet;
            private string _input;
            private char _char;
            private int _index;
            private long? _number;
            private char _numberChar;
            private int _numbersRead;

            public HashDecoder(Hashids parent, Span<long> buffer, string input)
            {
                _parent = parent;
                _buffer = buffer;
                _alphabet = null;
                _input = input;
                _char = '\0';
                _index = 0;
                _number = null;
                _numberChar = '\0';
                _numbersRead = 0;
            }

            public static long[] Decode(Hashids parent, string input)
            {
                int numbersCount = GetNumbersCount(parent, input);

                if (numbersCount == 0)
                    return Array.Empty<long>();

                long[] buffer = new long[numbersCount];
                HashDecoder hashDecoder = new HashDecoder(parent, buffer, input);

                return hashDecoder.TryDecode() ? buffer : Array.Empty<long>();
            }

            public static long DecondSingle(Hashids parent, string input)
            {
                int numbersCount = GetNumbersCount(parent, input);

                if (numbersCount == 0)
                    throw new NoResultException("The hash provided yielded no result.");

                if (numbersCount > 1)
                    throw new MultipleResultsException("The hash provided yielded more than one result.");

                Span<long> buffer = stackalloc long[1];
                HashDecoder hashDecoder = new HashDecoder(parent, buffer, input);

                if (hashDecoder.TryDecode())
                    return buffer[0];

                throw new NoResultException("The hash provided yielded no result.");
            }

            public static bool TryDecodeSingle(Hashids parent, string input, out long id)
            {
                id = 0L;

                if (GetNumbersCount(parent, input) != 1)
                    return false;

                Span<long> buffer = stackalloc long[1];
                HashDecoder hashDecoder = new HashDecoder(parent, buffer, input);

                if (hashDecoder.TryDecode())
                {
                    id = buffer[0];
                    return true;
                }

                return false;
            }

            private static int GetNumbersCount(Hashids parent, string input)
            {
                if (string.IsNullOrEmpty(input))
                    return 0;

                int count = 1;

                for (int i = 0; i < input.Length; i++)
                {
                    if (parent._seps.Contains(input[i]))
                        count++;
                }

                return count;
            }

            public bool TryDecode()
            {
                if (_input.Length < _parent._minHashLength)
                    return false;

                ReadIdle();

                if (!Read())
                    return false;

                _alphabet = _parent._alphabetProvider.GetAlphabet(_char);

                if (_alphabet == null)
                    return false;

                bool valid = ReadNumbers() && ValidateIdle();

                _alphabet.Return();

                return valid;
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

            private void ReadIdle()
            {
                while (Read())
                {
                    if (IsGuard())
                        return;
                }

                _index = 0;
            }

            private bool ReadNumbers()
            {
                while (Read() && !IsGuard())
                {
                    if (IsSeparator())
                    {
                        if (!ValidateSepartor())
                            return false;

                        Push(_number);

                        continue;
                    }

                    if (_number == null)
                    {
                        _number = 0L;
                        _numberChar = _char;

                        _alphabet = _alphabet.NextPage();
                    }

                    int charIndex = _alphabet.GetIndex(_char);

                    if (charIndex == -1)
                        return false;

                    _number *= _alphabet.Length;
                    _number += charIndex;
                }

                Push(_number);
                return true;
            }

            private void Push(long? number)
            {
                if (_number != null)
                    _buffer[_numbersRead++] = number.GetValueOrDefault();

                _number = null;
                _numberChar = '\0';
            }

            private bool IsGuard()
            {
                return _parent._guards.Contains(_char);
            }

            private bool IsSeparator()
            {
                return _parent._seps.Contains(_char);
            }

            private bool ValidateSepartor()
            {
                char separator = _parent.GetSeparator(_number.GetValueOrDefault(), _numbersRead, _numberChar);

                return separator == _char;
            }

            private bool ValidateIdle()
            {
                if (_input.Length == _index)
                    return true;

                Span<char> buffer = _input.Length >= 512 ? stackalloc char[_input.Length] : new char[_input.Length];

                _input.AsSpan().CopyTo(buffer);

                HashStats stats = HashStats.Create(_parent, _buffer);
                EncodingContext context = new EncodingContext(_parent, stats, buffer, _alphabet);

                IdleWriter.Write(ref context);

                _alphabet = context.Alphabet;

                return _input.AsSpan().SequenceEqual(buffer);
            }
        }
    }
}
