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
            private bool _endOfInput;
            private int _numbersRead;

            public HashDecoder(Hashids parent, Span<long> buffer, string input)
            {
                _parent = parent;
                _buffer = buffer;
                _alphabet = null;
                _input = input;
                _char = '\0';
                _index = 0;
                _endOfInput = false;
                _numbersRead = 0;
            }

            public static long[] Decode(Hashids parent, string input)
            {
                if (string.IsNullOrEmpty(input))
                    return Array.Empty<long>();

                int bufferSize = input.Length / 2;

                Span<long> buffer = bufferSize <= 512 ? stackalloc long[bufferSize] : new long[bufferSize];
                HashDecoder hashDecoder = new HashDecoder(parent, buffer, input);

                if (!hashDecoder.TryDecode())
                    return Array.Empty<long>();

                return buffer.Slice(0, hashDecoder._numbersRead).ToArray();
            }

            public static long DecondSingle(Hashids parent, string input)
            {
                Span<long> buffer = stackalloc long[1];
                HashDecoder hashDecoder = new HashDecoder(parent, buffer, input);

                if (!hashDecoder.TryDecode())
                    throw new NoResultException("The hash provided yielded no result.");

                if (hashDecoder._numbersRead > 1)
                    throw new MultipleResultsException("The hash provided yielded more than one result.");

                return buffer[0];
            }

            public static bool TryDecodeSingle(Hashids parent, string input, out long id)
            {
                Span<long> buffer = stackalloc long[1];
                HashDecoder hashDecoder = new HashDecoder(parent, buffer, input);

                if (hashDecoder.TryDecode() && hashDecoder._numbersRead == 1)
                {
                    id = buffer[0];
                    return true;
                }

                id = 0L;
                return false;
            }

            public bool TryDecode()
            {
                if (!ValidateHashLength())
                    return false;

                if (!Read())
                    return false;

                _alphabet = _parent._alphabetProvider.GetAlphabet(_char);

                if (_alphabet == null)
                    return false;

                bool valid = TryReadNumbers() && _numbersRead > 0 && ValidateIdle();

                _alphabet.Return();

                return valid;
            }

            private bool ValidateHashLength()
            {
                if (_parent._minHashLength == 0)
                    return true;

                if (_input.Length < _parent._minHashLength)
                    return false;

                while (Read())
                {
                    if (IsGuard())
                        return true;
                }

                _index = 0;
                _endOfInput = false;

                return true;
            }

            private bool Read()
            {
                if (_index >= _input.Length)
                {
                    _char = default(char);
                    _endOfInput = true;
                    return false;
                }

                _char = _input[_index++];
                return true;
            }

            private bool TryReadNumbers()
            {
                long? number = null;
                char separatorSalt = '\0';

                while (Read())
                {
                    if (number == null)
                    {
                        _alphabet = _alphabet.NextPage();

                        number = 0L;
                        separatorSalt = _char;
                    }

                    int charIndex = _alphabet.GetIndex(_char);

                    if (charIndex >= 0)
                    {
                        number = number.GetValueOrDefault() * _alphabet.Length + charIndex;

                        continue;
                    }

                    if (number == null)
                        return false;

                    Push(number.GetValueOrDefault());

                    if (_parent._minHashLength > 0 && IsGuard())
                        return true;

                    char separator = _parent.GetSeparator(number.GetValueOrDefault(), _numbersRead - 1, separatorSalt);

                    if (separator != _char)
                        return false;

                    number = null;
                }

                if (number != null)
                    Push(number.GetValueOrDefault());

                return true;
            }

            private void Push(long number)
            {
                if (_numbersRead < _buffer.Length)
                    _buffer[_numbersRead] = number;

                _numbersRead++;
            }

            private bool IsGuard()
            {
                return Array.IndexOf(_parent._guards, _char) >= 0;
            }

            private bool ValidateIdle()
            {
                if (_endOfInput)
                    return true;

                Span<char> buffer = _input.Length <= 512 ? stackalloc char[_input.Length] : new char[_input.Length];

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
