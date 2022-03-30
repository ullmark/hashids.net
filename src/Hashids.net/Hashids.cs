using System.Collections.Generic;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Buffers;

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

            InitCharArrays(alphabet: alphabet, seps: seps, salt: _salt, alphabetChars: out _alphabet, sepChars: out _seps, guardChars: out _guards);
        }

        /// <remarks>This method uses <c>out</c> params instead of returning a ValueTuple so it works with .NET 4.6.1.</remarks>
        private static void InitCharArrays(string alphabet, string seps, ReadOnlySpan<char> salt, out char[] alphabetChars, out char[] sepChars, out char[] guardChars)
        {
            alphabetChars = alphabet.ToCharArray().Distinct().ToArray();
            sepChars      = seps.ToCharArray();

            if (alphabetChars.Length < MIN_ALPHABET_LENGTH)
            {
                throw new ArgumentException($"Alphabet must contain at least {MIN_ALPHABET_LENGTH:N0} unique characters.", paramName: nameof(alphabet));
            }

            // SetupGuards():

            // seps should contain only characters present in alphabet:
            if (sepChars.Length > 0)
            {
                sepChars = sepChars.Intersect(alphabetChars).ToArray();
            }
            
            // alphabet should not contain seps // TODO: This comment contradicts the above, it needs rephrasing.
            if (sepChars.Length > 0 )
            {
                alphabetChars = alphabetChars.Except(sepChars).ToArray();
            }

            if (alphabetChars.Length < (MIN_ALPHABET_LENGTH - 6))
            {
                throw new ArgumentException($"Alphabet must contain at least {MIN_ALPHABET_LENGTH:N0} unique characters that are also not present in .", paramName: nameof(alphabet));
            }

            ConsistentShuffle(alphabet: sepChars, alphabetLength: sepChars.Length, salt: salt, saltLength: salt.Length);

            if (sepChars.Length == 0 || ((float)alphabetChars.Length / sepChars.Length) > SEP_DIV)
            {
                var sepsLength = (int)Math.Ceiling((float)alphabetChars.Length / SEP_DIV);

                if (sepsLength == 1)
                {
                    sepsLength = 2;
                }

                if (sepsLength > sepChars.Length)
                {
                    var diff = sepsLength - sepChars.Length;
                    sepChars = sepChars.Append(alphabetChars, 0, diff);
                    alphabetChars = alphabetChars.SubArray(diff);
                }
                else
                {
                    sepChars = sepChars.SubArray(0, sepsLength);
                }
            }

            ConsistentShuffle(alphabet: alphabetChars, alphabetChars.Length, salt: salt, salt.Length);
            
            // SetupGuards():
           
            var guardCount = (int)Math.Ceiling(alphabetChars.Length / GUARD_DIV);

            if (alphabetChars.Length < 3)
            {
                guardChars = sepChars.SubArray(index: 0, length: guardCount);
                sepChars   = sepChars.SubArray(index: guardCount);
            }

            else
            {
                guardChars    = alphabetChars.SubArray(index: 0, length: guardCount);
                alphabetChars = alphabetChars.SubArray(index: guardCount);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Encodes the provided number into a hashed string
        /// </summary>
        /// <param name="number">the number</param>
        /// <returns>the hashed string</returns>
        public string Encode(int number) => EncodeLong(number);
#endif

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

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Encodes the provided number into a hashed string
        /// </summary>
        /// <param name="number">the number</param>
        /// <returns>the hashed string</returns>
        public string EncodeLong(long number)
        {
            ReadOnlySpan<long> span = stackalloc[] { number };

            return GenerateHashFrom(span);
        }
#endif

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
            else
            {
                id = 0L;
                return false;
            }
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
            else
            {
                id = 0;
                return false;
            }
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
            if (numbers.Length == 0 || numbers.Any(n => n < 0))
                return string.Empty;

            long numbersHashInt = 0;
            for (var i = 0; i < numbers.Length; i++)
            {
                numbersHashInt += numbers[i] % (i + 100);
            }

            var builder = _sbPool.Get();

            char[] shuffleBuffer = null;
            var alphabet = _alphabet.CopyPooled();
            var hashBuffer = ArrayPool<char>.Shared.Rent(MaxNumberHashLength);
            try
            {
                var lottery = alphabet[numbersHashInt % _alphabet.Length];
                builder.Append(lottery);
                shuffleBuffer = CreatePooledBuffer(_alphabet.Length, lottery);

                var startIndex = 1 + _salt.Length;
                var length = _alphabet.Length - startIndex;

                for (var i = 0; i < numbers.Length; i++)
                {
                    var number = numbers[i];

                    if (length > 0)
                    {
                        Array.Copy(alphabet, 0, shuffleBuffer, startIndex, length);
                    }

                    ConsistentShuffle(alphabet, _alphabet.Length, shuffleBuffer, _alphabet.Length);
                    var hashLength = BuildReversedHash(number, alphabet, hashBuffer);

                    for (var j = hashLength - 1; j > -1; j--)
                    {
                        builder.Append(hashBuffer[j]);
                    }

                    if (i + 1 < numbers.Length)
                    {
                        number %= hashBuffer[hashLength - 1] + i;
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
                    Array.Copy(alphabet, shuffleBuffer, _alphabet.Length);
                    ConsistentShuffle(alphabet, _alphabet.Length, shuffleBuffer, _alphabet.Length);
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
                shuffleBuffer.ReturnToPool();
                hashBuffer.ReturnToPool();
            }

            var result = builder.ToString();
            _sbPool.Return(builder);
            return result;
        }

        private int BuildReversedHash(long input, ReadOnlySpan<char> alphabet, char[] hashBuffer)
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

            var i = (hashArray.Length is 3 or 2 ) ? 1 : 0;

            var hashBreakdown = hashArray[i];
            var lottery = hashBreakdown[0];

            if (lottery == '\0') /* default(char) == '\0' */
                return Array.Empty<long>();

            hashBreakdown = hashBreakdown.Substring(1);

            hashArray = hashBreakdown.Split(_seps, StringSplitOptions.RemoveEmptyEntries);

            var result = new long[hashArray.Length];
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
                    result[j] = Unhash(subHash, alphabet);
                }
            }
            finally
            {
                alphabet.ReturnToPool();
                buffer.ReturnToPool();
            }

            if (EncodeLong(result) == hash)
            {
                return result;
            }

            return Array.Empty<long>();
        }

        private char[] CreatePooledBuffer(int alphabetLength, char lottery)
        {
            var buffer = ArrayPool<char>.Shared.Rent(alphabetLength);
            buffer[0] = lottery;
            Array.Copy(_salt, 0, buffer, 1, Math.Min(_salt.Length, alphabetLength - 1));
            return buffer;
        }

        /// <summary>NOTE: This method mutates the <paramref name="alphabet"/> argument in-place.</summary>
        private static void ConsistentShuffle(char[] alphabet, int alphabetLength, ReadOnlySpan<char> salt, int saltLength)
        {
            if (salt.Length == 0)
                return;

            // TODO: Document or rename these cryptically-named variables: i, v, p, n.
            int n;
            for (int i = alphabetLength - 1, v = 0, p = 0; i > 0; i--, v++)
            {
                v %= saltLength;
                n = salt[v];
                p += n;
                var j = (n + v + p) % i;

                // swap characters at positions i and j:
                var temp = alphabet[j];
                alphabet[j] = alphabet[i];
                alphabet[i] = temp;
            }
        }
    }
}
