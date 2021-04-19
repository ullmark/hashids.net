﻿using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HashidsNet
{
    /// <summary>
    /// Generate YouTube-like hashes from one or many numbers. Use hashids when you do not want to expose your database ids to the user.
    /// </summary>
    public class Hashids : IHashids
    {
        public const string DEFAULT_ALPHABET = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        public const string DEFAULT_SEPS = "cfhistuCFHISTU";
        public const int MIN_ALPHABET_LENGTH = 16;

        private static readonly long[] EmptyArray = new long[0];
        
        private const double SEP_DIV = 3.5;
        private const double GUARD_DIV = 12.0;

        private char[] _alphabet;
        private char[] _salt;
        private char[] _seps;
        private char[] _guards;
        private int _minHashLength;

        //  Creates the Regex in the first usage, speed up first use of non hex methods
#if NETSTANDARD1_0
        private static Lazy<Regex> hexValidator = new Lazy<Regex>(() => new Regex("^[0-9a-fA-F]+$"));
        private static Lazy<Regex> hexSplitter = new Lazy<Regex>(() => new Regex(@"[\w\W]{1,12}"));
#else
        private static Lazy<Regex> hexValidator = new Lazy<Regex>(() => new Regex("^[0-9a-fA-F]+$", RegexOptions.Compiled));
        private static Lazy<Regex> hexSplitter = new Lazy<Regex>(() => new Regex(@"[\w\W]{1,12}", RegexOptions.Compiled));
#endif

        /// <summary>
        /// Instantiates a new Hashids with the default setup.
        /// </summary>
        public Hashids() : this(string.Empty, 0, DEFAULT_ALPHABET, DEFAULT_SEPS)
        {}

        /// <summary>
        /// Instantiates a new Hashids en/de-coder.
        /// </summary>
        /// <param name="salt"></param>
        /// <param name="minHashLength"></param>
        /// <param name="alphabet"></param>
        public Hashids(string salt = "", int minHashLength = 0, string alphabet = DEFAULT_ALPHABET, string seps = DEFAULT_SEPS)
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
                throw new ArgumentException($"Alphabet must contain atleast {MIN_ALPHABET_LENGTH} unique characters.", nameof(alphabet));

            SetupSeps();
            SetupGuards();
        }

        /// <summary>
        /// Encodes the provided numbers into a hashed string.
        /// </summary>
        /// <param name="numbers">The numbers to encode.</param>
        /// <returns>The hashed string.</returns>
        public virtual string Encode(params int[] numbers)
        {
            if (numbers.Any(n => n < 0)) 
                return string.Empty;
#if NETSTANDARD1_0
            return this.GenerateHashFrom(numbers.Select(n => (long)n).ToArray());
#else
            return GenerateHashFrom(Array.ConvertAll(numbers, n => (long)n));
#endif
        }

        /// <summary>
        /// Encodes the provided numbers into a hashed string.
        /// </summary>
        /// <param name="numbers">The numbers to encode.</param>
        /// <returns>The hashed string.</returns>
        public virtual string Encode(IEnumerable<int> numbers)
        {
            return Encode(numbers.ToArray());
        }

        /// <summary>
        /// Decodes the provided hash into.
        /// </summary>
        /// <param name="hash">The hash.</param>
        /// <exception cref="T:System.OverflowException">If the decoded number overflows integer.</exception>
        /// <returns>The numbers.</returns>
        public virtual int[] Decode(string hash)
        {
            var numbers = GetNumbersFrom(hash);
#if NETSTANDARD1_0
            return this.GetNumbersFrom(hash).Select(n => (int)n).ToArray();
#else
            return Array.ConvertAll(numbers, n => (int)n);
#endif
        }

        /// <summary>
        /// Encodes the provided hex string to a hashids hash.
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
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
        /// <param name="hash"></param>
        /// <returns></returns>
        public virtual string DecodeHex(string hash)
        {
            var builder = new StringBuilder();
            var numbers = DecodeLong(hash);

            foreach (var number in numbers)
            {
                builder.Append(string.Format("{0:X}", number).Substring(1));
            }

            return builder.ToString();
        }

        /// <summary>
        /// Decodes the provided hashed string into an array of longs.
        /// </summary>
        /// <param name="hash">The hashed string.</param>
        /// <returns>The numbers.</returns>
        public long[] DecodeLong(string hash)
        {
            return GetNumbersFrom(hash);
        }

        /// <summary>
        /// Encodes the provided longs to a hashed string.
        /// </summary>
        /// <param name="numbers">The numbers.</param>
        /// <returns>The hashed string.</returns>
        public string EncodeLong(params long[] numbers)
        {
            if (numbers.Any(n => n < 0)) 
                return string.Empty;

            return GenerateHashFrom(numbers);
        }

        /// <summary>
        /// Encodes the provided longs to a hashed string.
        /// </summary>
        /// <param name="numbers">The numbers.</param>
        /// <returns>The hashed string.</returns>
        public string EncodeLong(IEnumerable<long> numbers)
        {
            return EncodeLong(numbers.ToArray());
        }

        /// <summary>
        /// Encodes the provided numbers into a string.
        /// </summary>
        /// <param name="number">The numbers.</param>
        /// <returns>The hash.</returns>
        [Obsolete("Use 'Encode' instead. The method was renamed to better explain what it actually does.")]
        public virtual string Encrypt(params int[] numbers)
        {
            return Encode(numbers);
        }

        /// <summary>
        /// Encrypts the provided hex string to a hashids hash.
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        [Obsolete("Use 'EncodeHex' instead. The method was renamed to better explain what it actually does.")]
        public virtual string EncryptHex(string hex)
        {
            return EncodeHex(hex);
        }

        /// <summary>
        /// Decodes the provided numbers into a array of numbers.
        /// </summary>
        /// <param name="hash">Hash.</param>
        /// <returns>Array of numbers.</returns>
        [Obsolete("Use 'Decode' instead. Method was renamed to better explain what it actually does.")]
        public virtual int[] Decrypt(string hash)
        {
            return Decode(hash);
        }

        /// <summary>
        /// Decodes the provided hash to a hex-string.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        [Obsolete("Use 'DecodeHex' instead. The method was renamed to better explain what it actually does.")]
        public virtual string DecryptHex(string hash)
        {
            return DecodeHex(hash);
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetupSeps()
        {
            // seps should contain only characters present in alphabet; 
            _seps = _seps.Intersect(_alphabet).ToArray();

            // alphabet should not contain seps.
            _alphabet = _alphabet.Except(_seps).ToArray();

            ConsistentShuffle(_seps, _seps.Length, _salt, _salt.Length);

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

            ConsistentShuffle(_alphabet, _alphabet.Length, _salt, _salt.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetupGuards()
        {
            var guardCount = (int)Math.Ceiling(_alphabet.Length / GUARD_DIV);

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
        /// Internal function that does the work of creating the hash.
        /// </summary>
        /// <param name="numbers"></param>
        /// <returns></returns>
        private string GenerateHashFrom(long[] numbers)
        {
            if (numbers == null || numbers.Length == 0)
                return string.Empty;
            
            long numbersHashInt = 0;
            for (var i = 0; i < numbers.Length; i++)
            {
                numbersHashInt += numbers[i] % (i + 100);
            }

            var builder = new StringBuilder();

            char[] alphabet, buffer = null;
            alphabet = _alphabet.CopyPooled();
            try
            {
                var lottery = alphabet[numbersHashInt % _alphabet.Length];
                builder.Append(lottery);
                buffer = CreateBuffer(_alphabet.Length, lottery);
            
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
#if NETSTANDARD1_0 || NET40
#else
                System.Buffers.ArrayPool<char>.Shared.Return(alphabet);

                if (buffer != null)
                {
                    System.Buffers.ArrayPool<char>.Shared.Return(buffer);
                }
#endif
            }

            return builder.ToString();
        }

        private char[] CreateBuffer(int alphabetLength, char lottery)
        {
#if NETSTANDARD1_0 || NET40
            var buffer = new char[alphabetLength];
#else
            var buffer = System.Buffers.ArrayPool<char>.Shared.Rent(alphabetLength);
#endif
            buffer[0] = lottery;
            Array.Copy(_salt, 0, buffer, 1, Math.Min(_salt.Length, alphabetLength - 1));
            return buffer;
        }

        private char[] Hash(long input, char[] alphabet, int alphabetLength)
        {
            var hash = new List<char>(4);

            do
            {
                hash.Add(alphabet[input % alphabetLength]);
                input /= alphabetLength;
            }
            while (input > 0);

            hash.Reverse();
            return hash.ToArray();
        }

        private long Unhash(string input, char[] alphabet, int alphabetLength)
        {
            long number = 0;

            for (var i = 0; i < input.Length; i++)
            {
                var pos = Array.IndexOf(alphabet, input[i]);
                number += (long)(pos * Math.Pow(alphabetLength, input.Length - i - 1));
            }

            return number;
        }

        private long[] GetNumbersFrom(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                return EmptyArray;

            
            var result = new List<long>();
            int i = 0;

            var hashArray = hash.Split(_guards, StringSplitOptions.RemoveEmptyEntries);

            if (hashArray.Length == 3 || hashArray.Length == 2)
            {
                i = 1;
            }

            var hashBreakdown = hashArray[i];
            if (hashBreakdown[0] != default(char))
            {
                var lottery = hashBreakdown[0];
                hashBreakdown = hashBreakdown.Substring(1);

                hashArray = hashBreakdown.Split(_seps, StringSplitOptions.RemoveEmptyEntries);

                char[] alphabet, buffer = null;
                alphabet = _alphabet.CopyPooled();
                try
                {
                    buffer = CreateBuffer(_alphabet.Length, lottery);

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
#if NETSTANDARD1_0 || NET40
#else
                    System.Buffers.ArrayPool<char>.Shared.Return(alphabet);
                    if (buffer != null)
                    {
                        System.Buffers.ArrayPool<char>.Shared.Return(buffer);
                    }
#endif
                }

                if (EncodeLong(result.ToArray()) != hash)
                {
                    result.Clear();
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alphabet"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        private void ConsistentShuffle(char[] alphabet, int alphabetLength, char[] salt, int saltLength)
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
