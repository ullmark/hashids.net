using System.Collections.Generic;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace HashidsNet
{
    /// <summary>
    /// Generates YouTube-like hashes from one or many numbers. Use hashids when you do not want to expose your database ids to the user.
    /// </summary>
    public partial class Hashids : IHashids
    {
        public const string DEFAULT_ALPHABET = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        public const string DEFAULT_SEPS = "cfhistuCFHISTU";
        public const int MIN_ALPHABET_LENGTH = 16;

        private const double SEP_DIV = 3.5;
        private const double GUARD_DIV = 12.0;

        private const int MaxNumberHashLength = 12; // Length of long.MaxValue;

        private readonly char[] _alphabet;
        private readonly char[] _seps;
        private readonly char[] _guards;
        private readonly char[] _salt;
        private readonly int _minHashLength;

        private readonly StringBuilderPool _sbPool = new();

        // Creates the Regex in the first usage, speed up first use of non-hex methods
        private static readonly Lazy<Regex> hexValidator = new(() => new Regex("^[0-9a-fA-F]+$", RegexOptions.Compiled));
        private static readonly Lazy<Regex> hexSplitter = new(() => new Regex(@"[\w\W]{1,12}", RegexOptions.Compiled));

        /// <summary>
        /// Instantiates a new Hashids encoder/decoder with defaults.
        /// </summary>
        public Hashids() : this(salt: string.Empty, minHashLength: 0, alphabet: DEFAULT_ALPHABET, seps: DEFAULT_SEPS)
        {
            // empty constructor with defaults needed to allow mocking of public methods
        }

        /// <summary>
        /// Instantiates a new Hashids encoder/decoder.
        /// All parameters are optional and will use defaults unless otherwise specified.
        /// </summary>
        /// <param name="salt"></param>
        /// <param name="minHashLength"></param>
        /// <param name="alphabet"></param>
        /// <param name="seps"></param>
        public Hashids(
            string salt = "",
            int minHashLength = 0,
            string alphabet = DEFAULT_ALPHABET,
            string seps = DEFAULT_SEPS)
        {
            if (salt == null)
                throw new ArgumentNullException(nameof(salt));
            if (string.IsNullOrWhiteSpace(alphabet))
                throw new ArgumentNullException(nameof(alphabet));
            if (minHashLength < 0)
                throw new ArgumentOutOfRangeException(nameof(minHashLength), "Value must be zero or greater.");
            if (string.IsNullOrWhiteSpace(seps))
                throw new ArgumentNullException(nameof(seps));

            _salt = salt.Trim().ToCharArray();
            _minHashLength = minHashLength;
            _alphabet = alphabet.ToCharArray().Distinct().ToArray();
            _seps = seps.ToCharArray();

            if (_alphabet.Length < MIN_ALPHABET_LENGTH)
            {
                throw new ArgumentException($"Alphabet must contain at least {MIN_ALPHABET_LENGTH:N0} unique characters.", paramName: nameof(alphabet));
            }

            // separator characters can only be chosen from the characters in the alphabet
            if (_seps.Length > 0)
            {
                _seps = _seps.Intersect(_alphabet).ToArray();
            }

            // once separator characters are chosen, they must be removed from the alphabet available for hash generation
            if (_seps.Length > 0)
            {
                _alphabet = _alphabet.Except(_seps).ToArray();
            }

            if (_alphabet.Length < (MIN_ALPHABET_LENGTH - 6))
            {
                throw new ArgumentException($"Alphabet must contain at least {MIN_ALPHABET_LENGTH:N0} unique characters that are also not present in .", paramName: nameof(alphabet));
            }

            ConsistentShuffle(alphabet: _seps, salt: _salt);

            if (_seps.Length == 0 || ((float)_alphabet.Length / _seps.Length) > SEP_DIV)
            {
                var sepsLength = (int)Math.Ceiling((float)_alphabet.Length / SEP_DIV);

                if (sepsLength == 1)
                {
                    sepsLength = 2;
                }

                if (sepsLength > _seps.Length)
                {
                    var diff = sepsLength - _seps.Length;
                    _seps = _seps.Append(_alphabet, 0, diff);
                    _alphabet = _alphabet.SubArray(diff);
                }
                else
                {
                    _seps = _seps.SubArray(0, sepsLength);
                }
            }

            ConsistentShuffle(alphabet: _alphabet, salt: _salt);

            var guardCount = (int)Math.Ceiling(_alphabet.Length / GUARD_DIV);

            if (_alphabet.Length < 3)
            {
                _guards = _seps.SubArray(index: 0, length: guardCount);
                _seps = _seps.SubArray(index: guardCount);
            }

            else
            {
                _guards = _alphabet.SubArray(index: 0, length: guardCount);
                _alphabet = _alphabet.SubArray(index: guardCount);
            }
        }

        /// <summary>
        /// Encodes the provided number into a hashed string
        /// </summary>
        /// <param name="number">the number</param>
        /// <returns>the hashed string</returns>
        public string Encode(int number) => EncodeLong(number);

        /// <summary>
        /// Encodes the provided numbers into a hash string.
        /// </summary>
        /// <param name="numbers">List of integers.</param>
        /// <returns>Encoded hash string.</returns>
        public virtual string Encode(params int[] numbers) => GenerateHashFrom(Array.ConvertAll(numbers, n => (long)n));

        /// <summary>
        /// Encodes the provided numbers into a hash string.
        /// </summary>
        /// <param name="numbers">Enumerable list of integers.</param>
        /// <returns>Encoded hash string.</returns>
        public virtual string Encode(IEnumerable<int> numbers) => Encode(numbers.ToArray());

        /// <summary>
        /// Encodes the provided number into a hashed string
        /// </summary>
        /// <param name="number">the number</param>
        /// <returns>the hashed string</returns>
        public string EncodeLong(long number) => GenerateHashFrom(stackalloc[] { number });

        /// <summary>
        /// Encodes the provided numbers into a hash string.
        /// </summary>
        /// <param name="numbers">List of 64-bit integers.</param>
        /// <returns>Encoded hash string.</returns>
        public string EncodeLong(params long[] numbers) => GenerateHashFrom(numbers);

        /// <summary>
        /// Encodes the provided numbers into a hash string.
        /// </summary>
        /// <param name="numbers">Enumerable list of 64-bit integers.</param>
        /// <returns>Encoded hash string.</returns>
        public string EncodeLong(IEnumerable<long> numbers) => EncodeLong(numbers.ToArray());

        /// <summary>
        /// Decodes the provided hash into numbers.
        /// </summary>
        /// <param name="hash">Hash string to decode.</param>
        /// <returns>Array of integers.</returns>
        /// <exception cref="T:System.OverflowException">If the decoded number overflows integer.</exception>
        public virtual int[] Decode(string hash) => Array.ConvertAll(GetNumbersFrom(hash), n => (int)n);

        /// <summary>
        /// Decodes the provided hash into numbers.
        /// </summary>
        /// <param name="hash">Hash string to decode.</param>
        /// <returns>Array of 64-bit integers.</returns>
        public long[] DecodeLong(string hash) => GetNumbersFrom(hash);

        /// <inheritdoc />
        public long DecodeSingleLong(string hash)
        {
            var numbers = GetNumbersFrom(hash);

            if (numbers.Length == 0)
                throw new NoResultException("The hash provided yielded no result.");

            if (numbers.Length > 1)
                throw new MultipleResultsException("The hash provided yielded more than one result.");

            return numbers[0];
        }

        /// <inheritdoc />
        public bool TryDecodeSingleLong(string hash, out long id)
        {
            var numbers = GetNumbersFrom(hash);

            if (numbers.Length == 1)
            {
                id = numbers[0];
                return true;
            }

            id = 0L;
            return false;
        }

        /// <inheritdoc />
        public virtual int DecodeSingle(string hash)
        {
            var numbers = GetNumbersFrom(hash);

            if (numbers.Length == 0)
                throw new NoResultException("The hash provided yielded no result.");

            if (numbers.Length > 1)
                throw new MultipleResultsException("The hash provided yielded more than one result.");

            return (int)numbers[0];
        }

        /// <inheritdoc />
        public virtual bool TryDecodeSingle(string hash, out int id)
        {
            var numbers = GetNumbersFrom(hash);

            if (numbers.Length == 1)
            {
                id = (int)numbers[0];
                return true;
            }

            id = 0;
            return false;
        }

        /// <summary>
        /// Encodes the provided hex-string into a hash string.
        /// </summary>
        /// <param name="hex">Hex string to encode.</param>
        /// <returns>Encoded hash string.</returns>
        public virtual string EncodeHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex) || !hexValidator.Value.IsMatch(hex))
                return string.Empty;

            var matches = hexSplitter.Value.Matches(hex);
            if (matches.Count == 0) return string.Empty;

            var numbers = new long[matches.Count];
            for (int i = 0; i < numbers.Length; i++)
            {
                Match match = matches[i];
                string concat = string.Concat("1", match.Value);
                var number = Convert.ToInt64(concat, fromBase: 16);
                numbers[i] = number;
            }

            return EncodeLong(numbers);
        }

        /// <summary>
        /// Decodes the provided hash into a hex-string.
        /// </summary>
        /// <param name="hash">Hash string to decode.</param>
        /// <returns>Decoded hex string.</returns>
        public virtual string DecodeHex(string hash)
        {
            var builder = _sbPool.Get();
            var numbers = DecodeLong(hash);

            foreach (var number in numbers)
            {
                var s = number.ToString("X");

                for (var i = 1; i < s.Length; i++)
                {
                    builder.Append(s[i]);
                }
            }

            var result = builder.ToString();
            _sbPool.Return(builder);
            return result;
        }

        private string GenerateHashFrom(ReadOnlySpan<long> numbers)
        {
            if (numbers.Length == 0)
                return string.Empty;

            foreach (var item in numbers)
                if (item < 0)
                    return string.Empty;

            long numbersHashInt = 0;
            for (var i = 0; i < numbers.Length; i++)
            {
                numbersHashInt += numbers[i] % (i + 100);
            }

            var stringBuilder = _sbPool.Get();

            Span<char> alphabet = _alphabet.Length < 512 ? stackalloc char[_alphabet.Length] : new char[_alphabet.Length];
            _alphabet.CopyTo(alphabet);

            var lottery = alphabet[(int)(numbersHashInt % _alphabet.Length)];
            stringBuilder.Append(lottery);

            Span<char> shuffleBuffer = _alphabet.Length < 512 ? stackalloc char[_alphabet.Length] : new char[_alphabet.Length];
            shuffleBuffer[0] = lottery;
            _salt.AsSpan().Slice(0, Math.Min(_salt.Length, _alphabet.Length - 1)).CopyTo(shuffleBuffer.Slice(1));

            var startIndex = 1 + _salt.Length;
            var length = _alphabet.Length - startIndex;

            Span<char> hashBuffer = stackalloc char[MaxNumberHashLength];

            for (var i = 0; i < numbers.Length; i++)
            {
                var number = numbers[i];

                if (length > 0)
                {
                    alphabet.Slice(0, length).CopyTo(shuffleBuffer.Slice(startIndex));
                }

                ConsistentShuffle(alphabet, shuffleBuffer);
                var hashLength = BuildReversedHash(number, alphabet, hashBuffer);

                for (var j = hashLength - 1; j > -1; j--)
                {
                    stringBuilder.Append(hashBuffer[j]);
                }

                if (i + 1 < numbers.Length)
                {
                    number %= hashBuffer[hashLength - 1] + i;
                    var sepsIndex = number % _seps.Length;

                    stringBuilder.Append(_seps[sepsIndex]);
                }
            }

            if (stringBuilder.Length < _minHashLength)
            {
                var guardIndex = (numbersHashInt + stringBuilder[0]) % _guards.Length;
                var guard = _guards[guardIndex];

                stringBuilder.Insert(0, guard);

                if (stringBuilder.Length < _minHashLength)
                {
                    guardIndex = (numbersHashInt + stringBuilder[2]) % _guards.Length;
                    guard = _guards[guardIndex];

                    stringBuilder.Append(guard);
                }
            }

            var halfLength = _alphabet.Length / 2;

            while (stringBuilder.Length < _minHashLength)
            {
                alphabet.CopyTo(shuffleBuffer);
                ConsistentShuffle(alphabet, shuffleBuffer);

#if NETSTANDARD2_0
                stringBuilder.Insert(0, alphabet.Slice(halfLength, _alphabet.Length - halfLength).ToArray());
                stringBuilder.Append(alphabet.Slice(0, halfLength).ToArray());
#else
                stringBuilder.Insert(0, alphabet[halfLength.._alphabet.Length]);
                stringBuilder.Append(alphabet[..halfLength]);
#endif

                var excess = stringBuilder.Length - _minHashLength;
                if (excess > 0)
                {
                    stringBuilder.Remove(0, excess / 2);
                    stringBuilder.Remove(_minHashLength, stringBuilder.Length - _minHashLength);
                }
            }

            var result = stringBuilder.ToString();
            _sbPool.Return(stringBuilder);
            return result;
        }

        private int BuildReversedHash(long input, ReadOnlySpan<char> alphabet, Span<char> hashBuffer)
        {
            var length = 0;
            do
            {
                int idx = (int)(input % _alphabet.Length);
                hashBuffer[length] = alphabet[idx];
                length += 1;
                input /= _alphabet.Length;
            }
            while (input > 0);

            return length;
        }

        private long Unhash(string input, ReadOnlySpan<char> alphabet)
        {
            long number = 0;

            for (var i = 0; i < input.Length; i++)
            {
                var pos = alphabet.IndexOf(input[i]);
                number = (number * _alphabet.Length) + pos;
            }

            return number;
        }

        private long[] GetNumbersFrom(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                return Array.Empty<long>();

            var hashArray = hash.Split(_guards, StringSplitOptions.RemoveEmptyEntries);
            if (hashArray.Length == 0)
                return Array.Empty<long>();

            var i = (hashArray.Length is 3 or 2) ? 1 : 0;

            var hashBreakdown = hashArray[i];
            var lottery = hashBreakdown[0];

            if (lottery == '\0') /* default(char) == '\0' */
                return Array.Empty<long>();

            hashBreakdown = hashBreakdown.Substring(1);

            hashArray = hashBreakdown.Split(_seps, StringSplitOptions.RemoveEmptyEntries);

            var result = new long[hashArray.Length];

            Span<char> alphabet = _alphabet.Length < 512 ? stackalloc char[_alphabet.Length] : new char[_alphabet.Length];
            _alphabet.CopyTo(alphabet);

            Span<char> buffer = _alphabet.Length < 512 ? stackalloc char[_alphabet.Length] : new char[_alphabet.Length];
            buffer[0] = lottery;
            _salt.AsSpan().Slice(0, Math.Min(_salt.Length, _alphabet.Length - 1)).CopyTo(buffer.Slice(1));

            var startIndex = 1 + _salt.Length;
            var length = _alphabet.Length - startIndex;

            for (var j = 0; j < hashArray.Length; j++)
            {
                var subHash = hashArray[j];

                if (length > 0)
                {
                    alphabet.Slice(0, length).CopyTo(buffer.Slice(startIndex));
                }

                ConsistentShuffle(alphabet, buffer);
                result[j] = Unhash(subHash, alphabet);
            }

            // regenerate hash from numbers and compare to given hash to ensure the correct parameters were used
            if (GenerateHashFrom(result).Equals(hash, StringComparison.Ordinal))
                return result;

            return Array.Empty<long>();
        }

        /// <summary>NOTE: This method mutates the <paramref name="alphabet"/> argument in-place.</summary>
        private static void ConsistentShuffle(Span<char> alphabet, ReadOnlySpan<char> salt)
        {
            if (salt.Length == 0)
                return;

            // TODO: Document or rename these cryptically-named variables: i, v, p, n.
            int n;
            for (int i = alphabet.Length - 1, v = 0, p = 0; i > 0; i--, v++)
            {
                v %= salt.Length;
                n = salt[v];
                p += n;
                var j = (n + v + p) % i;

                // swap characters at positions i and j:
                (alphabet[i], alphabet[j]) = (alphabet[j], alphabet[i]);
            }
        }
    }
}
