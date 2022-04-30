using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HashidsNet.Alphabets;
using HashidsNet.Alphabets.Salts;

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

        private readonly char[] _alphabet;
        private readonly char[] _seps;
        private readonly char[] _guards;
        private readonly char[] _salt;
        private readonly int _minHashLength;

        private readonly IAlphabetProvider _alphabetProvider;

        // Creates the Regex in the first usage, speed up first use of non-hex methods
        private static readonly Lazy<Regex> HexValidator = new(() => new Regex("^[0-9a-fA-F]+$", RegexOptions.Compiled));
        private static readonly Lazy<Regex> HexSplitter = new(() => new Regex(@"[\w\W]{1,12}", RegexOptions.Compiled));
        private readonly StringBuilderPool StringBuilderPool = new();

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
            string seps = DEFAULT_SEPS,
            bool useCache = true)
        {
            if (salt == null) throw new ArgumentNullException(nameof(salt));
            if (minHashLength < 0) throw new ArgumentOutOfRangeException(nameof(minHashLength), "Value must be zero or greater.");
            if (string.IsNullOrWhiteSpace(alphabet)) throw new ArgumentNullException(nameof(alphabet));
            if (string.IsNullOrWhiteSpace(seps)) throw new ArgumentNullException(nameof(seps));

            _salt = salt.Trim().ToCharArray();
            _minHashLength = minHashLength;
            _alphabet = alphabet.ToCharArray().Distinct().ToArray();
            _seps = seps.ToCharArray();

            if (_alphabet.Length < MIN_ALPHABET_LENGTH)
                throw new ArgumentException($"Alphabet must contain at least {MIN_ALPHABET_LENGTH:N0} unique characters.", paramName: nameof(alphabet));

            // separator characters can only be chosen from the characters in the alphabet
            if (_seps.Length > 0)
                _seps = _seps.Intersect(_alphabet).ToArray();

            // once separator characters are chosen, they must be removed from the alphabet available for hash generation
            if (_seps.Length > 0)
                _alphabet = _alphabet.Except(_seps).ToArray();

            if (_alphabet.Length < (MIN_ALPHABET_LENGTH - 6))
                throw new ArgumentException($"Alphabet must contain at least {MIN_ALPHABET_LENGTH:N0} unique characters that are also not present in .", paramName: nameof(alphabet));

            Salt.Create(_salt).Shuffle(_seps);

            if (_seps.Length == 0 || ((float)_alphabet.Length / _seps.Length) > SEP_DIV)
            {
                var sepsLength = (int)Math.Ceiling((float)_alphabet.Length / SEP_DIV);

                if (sepsLength == 1)
                    sepsLength = 2;

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

            Salt.Create(_salt).Shuffle(_alphabet);

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

            _alphabetProvider = useCache ?
                new CacheAlphabetProvider(_alphabet, _salt) :
                new AlphabetProvider(_alphabet, _salt);
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
        public virtual string Encode(params int[] numbers) => EncodeLong(Array.ConvertAll(numbers, n => (long)n));

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
        public string EncodeLong(long number) => HashEncoder.Encode(this, number);

        /// <summary>
        /// Encodes the provided numbers into a hash string.
        /// </summary>
        /// <param name="numbers">List of 64-bit integers.</param>
        /// <returns>Encoded hash string.</returns>
        public string EncodeLong(params long[] numbers) => HashEncoder.Encode(this, numbers);

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
        public virtual int[] Decode(string hash) => Array.ConvertAll(DecodeLong(hash), n => (int)n);

        /// <summary>
        /// Decodes the provided hash into numbers.
        /// </summary>
        /// <param name="hash">Hash string to decode.</param>
        /// <returns>Array of 64-bit integers.</returns>
        public long[] DecodeLong(string hash) => HashDecoder.Decode(this, hash);

        /// <inheritdoc />
        public long DecodeSingleLong(string hash)
        {
            return HashDecoder.DecondSingle(this, hash);
        }

        /// <inheritdoc />
        public bool TryDecodeSingleLong(string hash, out long id)
        {
            return HashDecoder.TryDecodeSingle(this, hash, out id);
        }

        /// <inheritdoc />
        public virtual int DecodeSingle(string hash)
        {
            return (int)DecodeSingleLong(hash);
        }

        /// <inheritdoc />
        public virtual bool TryDecodeSingle(string hash, out int id)
        {
            long idLong;
            if (TryDecodeSingleLong(hash, out idLong))
            {
                id = (int)idLong;
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
            if (string.IsNullOrWhiteSpace(hex) || !HexValidator.Value.IsMatch(hex))
                return string.Empty;

            var matches = HexSplitter.Value.Matches(hex);
            if (matches.Count == 0) return string.Empty;

            var numbers = new long[matches.Count];
            for (int i = 0; i < numbers.Length; i++)
            {
                var match = matches[i];
                var concat = string.Concat("1", match.Value);
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
            var builder = StringBuilderPool.Get();
            var numbers = DecodeLong(hash);

            foreach (var number in numbers)
            {
                var s = number.ToString("X");
                for (var i = 1; i < s.Length; i++)
                    builder.Append(s[i]);
            }

            var result = builder.ToString();
            StringBuilderPool.Return(builder);
            return result;
        }

        private char GetSeparator(long number, int numberIndex, char salt)
        {
            int index = (int)(number % (salt + numberIndex) % _seps.Length);

            return _seps[index];
        }
    }
}
