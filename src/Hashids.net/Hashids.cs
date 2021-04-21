using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.ObjectPool;

namespace HashidsNet
{
    /// <summary>
    /// Generate YouTube-like hashes from one or many numbers. Use hashids when you do not want to expose your database ids to the user.
    /// </summary>
    public partial class Hashids : IHashids
    {
        public const string DEFAULT_ALPHABET = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        public const string DEFAULT_SEPS = "cfhistuCFHISTU";
        public const int MIN_ALPHABET_LENGTH = 16;

        private const double SEP_DIV = 3.5;
        private const double GUARD_DIV = 12.0;

        private char[] _alphabet;
        private char[] _seps;
        private char[] _guards;
        private char[] _salt;
        private readonly int _minHashLength;

        private readonly ObjectPool<StringBuilder> _sbPool = new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());

        // Creates the Regex in the first usage, speed up first use of non-hex methods
        private static readonly Lazy<Regex> hexValidator = new(() => new Regex("^[0-9a-fA-F]+$", RegexOptions.Compiled));
        private static readonly Lazy<Regex> hexSplitter = new(() => new Regex(@"[\w\W]{1,12}", RegexOptions.Compiled));

        /// <summary>
        /// Instantiates a new Hashids encoder/decoder with defaults.
        /// </summary>
        public Hashids() : this(string.Empty, 0, DEFAULT_ALPHABET, DEFAULT_SEPS)
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
            _alphabet = alphabet.ToCharArray().Distinct().ToArray();
            _seps = seps.ToCharArray();
            _minHashLength = minHashLength;

            if (_alphabet.Length < MIN_ALPHABET_LENGTH)
                throw new ArgumentException($"Alphabet must contain at least {MIN_ALPHABET_LENGTH} unique characters.",
                    nameof(alphabet));

            SetupSeps();
            SetupGuards();
        }

        private void SetupSeps()
        {
            // seps should contain only characters present in alphabet; 
            _seps = _seps.Intersect(_alphabet).ToArray();

            // alphabet should not contain seps.
            _alphabet = _alphabet.Except(_seps).ToArray();

            ConsistentShuffle(_seps, _seps.Length, _salt, _salt.Length);

            if (_seps.Length == 0 || ((float) _alphabet.Length / _seps.Length) > SEP_DIV)
            {
                var sepsLength = (int) Math.Ceiling((float) _alphabet.Length / SEP_DIV);

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

            ConsistentShuffle(_alphabet, _alphabet.Length, _salt, _salt.Length);
        }

        private void SetupGuards()
        {
            var guardCount = (int) Math.Ceiling(_alphabet.Length / GUARD_DIV);

            if (_alphabet.Length < 3)
            {
                _guards = _seps.SubArray(0, guardCount);
                _seps = _seps.SubArray(guardCount);
            }

            else
            {
                _guards = _alphabet.SubArray(0, guardCount);
                _alphabet = _alphabet.SubArray(guardCount);
            }
        }

        /// <summary>
        /// Encodes the provided numbers into a hash string.
        /// </summary>
        /// <param name="numbers">List of integers.</param>
        /// <returns>Encoded hash string.</returns>
        public virtual string Encode(params int[] numbers) => GenerateHashFrom(Array.ConvertAll(numbers, n => (long) n));

        /// <summary>
        /// Encodes the provided numbers into a hash string.
        /// </summary>
        /// <param name="numbers">Enumerable list of integers.</param>
        /// <returns>Encoded hash string.</returns>
        public virtual string Encode(IEnumerable<int> numbers) => Encode(numbers.ToArray());

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
        public virtual int[] Decode(string hash) => Array.ConvertAll(GetNumbersFrom(hash), n => (int) n);

        /// <summary>
        /// Decodes the provided hash into numbers.
        /// </summary>
        /// <param name="hash">Hash string to decode.</param>
        /// <returns>Array of 64-bit integers.</returns>
        public long[] DecodeLong(string hash) => GetNumbersFrom(hash);

        /// <summary>
        /// Encodes the provided hex-string into a hash string.
        /// </summary>
        /// <param name="hex">Hex string to encode.</param>
        /// <returns>Encoded hash string.</returns>
        public virtual string EncodeHex(string hex)
        {
            if (!hexValidator.Value.IsMatch(hex))
                return string.Empty;

            var matches = hexSplitter.Value.Matches(hex);
            var numbers = new List<long>(matches.Count);

            foreach (Match match in matches)
            {
                var number = Convert.ToInt64(string.Concat("1", match.Value), 16);
                numbers.Add(number);
            }

            return EncodeLong(numbers.ToArray());
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
            foreach (var ch in number.ToString("X").AsSpan().Slice(1))
                builder.Append(ch);

            var result = builder.ToString();
            _sbPool.Return(builder);
            return result;
        }

        private string GenerateHashFrom(long[] numbers)
        {
            if (numbers == null || numbers.Length == 0 || numbers.Any(n => n < 0))
                return string.Empty;

            long numbersHashInt = 0;
            for (var i = 0; i < numbers.Length; i++)
            {
                numbersHashInt += numbers[i] % (i + 100);
            }

            var builder = _sbPool.Get();

            char[] buffer = null;
            var alphabet = _alphabet.CopyPooled();
            try
            {
                var lottery = alphabet[numbersHashInt % _alphabet.Length];
                builder.Append(lottery);
                buffer = CreatePooledBuffer(_alphabet.Length, lottery);

                var startIndex = 1 + _salt.Length;
                var length = _alphabet.Length - startIndex;

                for (var i = 0; i < numbers.Length; i++)
                {
                    var number = numbers[i];

                    if (length > 0)
                    {
                        Array.Copy(alphabet, 0, buffer, startIndex, length);
                    }

                    ConsistentShuffle(alphabet, _alphabet.Length, buffer, _alphabet.Length);
                    var last = Hash(number, alphabet, _alphabet.Length);

                    builder.Append(last);

                    if (i + 1 < numbers.Length)
                    {
                        number %= last[0] + i;
                        var sepsIndex = number % _seps.Length;

                        builder.Append(_seps[sepsIndex]);
                    }
                }

                if (builder.Length < _minHashLength)
                {
                    var guardIndex = (numbersHashInt + builder[0]) % _guards.Length;
                    var guard = _guards[guardIndex];

                    builder.Insert(0, guard);

                    if (builder.Length < _minHashLength)
                    {
                        guardIndex = (numbersHashInt + builder[2]) % _guards.Length;
                        guard = _guards[guardIndex];

                        builder.Append(guard);
                    }
                }

                var halfLength = _alphabet.Length / 2;

                while (builder.Length < _minHashLength)
                {
                    Array.Copy(alphabet, buffer, _alphabet.Length);
                    ConsistentShuffle(alphabet, _alphabet.Length, buffer, _alphabet.Length);
                    builder.Insert(0, alphabet, halfLength, _alphabet.Length - halfLength);
                    builder.Append(alphabet, 0, halfLength);

                    var excess = builder.Length - _minHashLength;
                    if (excess > 0)
                    {
                        builder.Remove(0, excess / 2);
                        builder.Remove(_minHashLength, builder.Length - _minHashLength);
                    }
                }
            }
            finally
            {
                alphabet.ReturnToPool();
                buffer.ReturnToPool();
            }

            var result = builder.ToString();
            _sbPool.Return(builder);
            return result;
        }

        private char[] Hash(long input, char[] alphabet, int alphabetLength)
        {
            var hash = new List<char>(4);

            do
            {
                hash.Add(alphabet[input % alphabetLength]);
                input /= alphabetLength;
            } while (input > 0);

            hash.Reverse();
            return hash.ToArray();
        }

        private long Unhash(string input, char[] alphabet, int alphabetLength)
        {
            long number = 0;

            for (var i = 0; i < input.Length; i++)
            {
                var pos = Array.IndexOf(alphabet, input[i]);
                number = number * alphabetLength + pos;
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

            var i = 0;
            if (hashArray.Length == 3 || hashArray.Length == 2)
            {
                i = 1;
            }

            var result = new List<long>();
            var hashBreakdown = hashArray[i];
            if (hashBreakdown[0] != default(char))
            {
                var lottery = hashBreakdown[0];
                hashBreakdown = hashBreakdown.Substring(1);

                hashArray = hashBreakdown.Split(_seps, StringSplitOptions.RemoveEmptyEntries);

                char[] buffer = null;
                var alphabet = _alphabet.CopyPooled();
                try
                {
                    buffer = CreatePooledBuffer(_alphabet.Length, lottery);

                    var startIndex = 1 + _salt.Length;
                    var length = _alphabet.Length - startIndex;

                    for (var j = 0; j < hashArray.Length; j++)
                    {
                        var subHash = hashArray[j];

                        if (length > 0)
                        {
                            Array.Copy(alphabet, 0, buffer, startIndex, length);
                        }

                        ConsistentShuffle(alphabet, _alphabet.Length, buffer, _alphabet.Length);
                        result.Add(Unhash(subHash, alphabet, _alphabet.Length));
                    }
                }
                finally
                {
                    alphabet.ReturnToPool();
                    buffer.ReturnToPool();
                }

                if (EncodeLong(result.ToArray()) != hash)
                {
                    result.Clear();
                }
            }

            return result.ToArray();
        }

        private char[] CreatePooledBuffer(int alphabetLength, char lottery)
        {
            var buffer = System.Buffers.ArrayPool<char>.Shared.Rent(alphabetLength);
            buffer[0] = lottery;
            Array.Copy(_salt, 0, buffer, 1, Math.Min(_salt.Length, alphabetLength - 1));
            return buffer;
        }

        private static void ConsistentShuffle(char[] alphabet, int alphabetLength, char[] salt, int saltLength)
        {
            if (salt.Length == 0)
                return;

            int n;
            for (int i = alphabetLength - 1, v = 0, p = 0; i > 0; i--, v++)
            {
                v %= saltLength;
                p += (n = salt[v]);
                var j = (n + v + p) % i;
                // swap characters at positions i and j
                var temp = alphabet[j];
                alphabet[j] = alphabet[i];
                alphabet[i] = temp;
            }
        }
    }
}