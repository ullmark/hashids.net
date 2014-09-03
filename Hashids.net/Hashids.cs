﻿using System.Collections.Generic;
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

        private const int MIN_ALPHABET_LENGTH = 16;
        private const double SEP_DIV = 3.5;
        private const double GUARD_DIV = 12.0;

        private string alphabet;
        private string salt;
		private string seps;
		private string guards;
        private int minHashLength;

        private Regex guardsRegex;
        private Regex sepsRegex;
        private static Regex hexValidator = new Regex("^[0-9a-fA-F]+$", RegexOptions.Compiled);
        private static Regex hexSplitter = new Regex(@"[\w\W]{1,12}", RegexOptions.Compiled);

        /// <summary>
        /// Instantiates a new Hashids en/de-crypter.
        /// </summary>
        /// <param name="salt"></param>
        /// <param name="minHashLength"></param>
        /// <param name="alphabet"></param>
		public Hashids(string salt = "", int minHashLength = 0, string alphabet = DEFAULT_ALPHABET, string seps = DEFAULT_SEPS)
		{
			if (string.IsNullOrWhiteSpace(alphabet))
				throw new ArgumentNullException("alphabet");

			this.salt = salt;
			this.alphabet = string.Join(string.Empty, alphabet.Distinct());
            this.seps = seps;
			this.minHashLength = minHashLength;

			if (this.alphabet.Length < 16)
				throw new ArgumentException("alphabet must contain atleast 4 unique characters.", "alphabet");

            SetupSeps();
            SetupGuards();
		}

        /// <summary>
        /// 
        /// </summary>
        private void SetupSeps()
        {
            // seps should contain only characters present in alphabet; 
            seps = new String(seps.Intersect(alphabet.ToArray()).ToArray());

            // alphabet should not contain seps.
            alphabet = new String(alphabet.Except(seps.ToArray()).ToArray());

            seps = ConsistentShuffle(seps, salt);

            if (seps.Length == 0 || (alphabet.Length / seps.Length) > SEP_DIV)
            {
                var sepsLength = (int)Math.Ceiling(alphabet.Length / SEP_DIV);
                if (sepsLength == 1)
                    sepsLength = 2;

                if (sepsLength > seps.Length)
                {
                    var diff = sepsLength - seps.Length;
                    seps += alphabet.Substring(0, diff);
                    alphabet = alphabet.Substring(diff);
                }

                else seps = seps.Substring(0, sepsLength);
            }

            sepsRegex = new Regex(string.Concat("[", seps, "]"), RegexOptions.Compiled);
            alphabet = ConsistentShuffle(alphabet, salt);
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetupGuards()
        {
            var guardCount = (int)Math.Ceiling(alphabet.Length / GUARD_DIV);

            if (alphabet.Length < 3)
            {
                guards = seps.Substring(0, guardCount);
                seps = seps.Substring(guardCount);
            }
            
            else
            {
                guards = alphabet.Substring(0, guardCount);
                alphabet = alphabet.Substring(guardCount);
            }

            guardsRegex = new Regex(string.Concat("[", guards, "]"), RegexOptions.Compiled);
        }

		/// <summary>
		/// Encrypts the provided numbers into a hash.
		/// </summary>
		/// <param name="numbers">the numbers</param>
		/// <returns>the hash</returns>
		public string Encrypt(params int[] numbers)
		{
            if (numbers == null || numbers.Length == 0)
                return string.Empty;

			return Encode(numbers);
		}

        /// <summary>
        /// Encrypts the provided hex string to a hashids hash.
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public string EncryptHex(string hex)
        {
            if (!hexValidator.IsMatch(hex))
                return string.Empty;

            var numbers = new List<int>();
            var matches = hexSplitter.Matches(hex);

            foreach(Match match in matches)
            {
                var number = Convert.ToInt32(string.Concat("1", match.Value), 16);
                numbers.Add(number);
            }

            return this.Encrypt(numbers.ToArray());
        }

		/// <summary>
		/// Decrypts the provided numbers into a array of numbers
		/// </summary>
		/// <param name="hash">hash</param>
		/// <returns>array of numbers.</returns>
		public int[] Decrypt(string hash)
		{
            if (string.IsNullOrWhiteSpace(hash))
                return new int[0];

			return Decode(hash, alphabet);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public string DecryptHex(string hash)
        {
            var ret = new StringBuilder();
            var numbers = this.Decrypt(hash);

            foreach (var number in numbers)
                ret.Append(string.Format("{0:X}", number).Substring(1));

            return ret.ToString();
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="numbers"></param>
		/// <param name="alphabet"></param>
		/// <param name="salt"></param>
		/// <param name="minHashLength"></param>
		/// <returns></returns>
		private string Encode(int[] numbers)
		{
            string ret;
            var alphabet = this.alphabet;

            int numbersHashInt = 0;
            for (var i = 0; i < numbers.Length; i++)
                numbersHashInt += (numbers[i] % (i + 100));

            var lottery = alphabet[numbersHashInt % alphabet.Length];
            ret = lottery.ToString();

            for (var i = 0; i < numbers.Length; i++)
            {
                var number = numbers[i];
                var buffer = lottery + this.salt + alphabet;

                alphabet = ConsistentShuffle(alphabet, buffer.Substring(0, alphabet.Length));
                var last = Hash(number, alphabet);

                ret += last;

                if (i + 1 < numbers.Length)
                {
                    number %= ((int)last[0] + i);
                    var sepsIndex = number % this.seps.Length;

                    ret += this.seps[sepsIndex];
                }
            }

            if (ret.Length < this.minHashLength)
            {
                var guardIndex = (numbersHashInt + (int)ret[0]) % this.guards.Length;
                var guard = this.guards[guardIndex];

                ret = guard + ret;

                if (ret.Length < this.minHashLength)
                {
                    guardIndex = (numbersHashInt + (int)ret[2]) % this.guards.Length;
                    guard = this.guards[guardIndex];

                    ret += guard;
                }
            }

            var halfLength = (int)(alphabet.Length / 2);
            while (ret.Length < this.minHashLength)
            {
                alphabet = ConsistentShuffle(alphabet, alphabet);
                ret = alphabet.Substring(halfLength) + ret + alphabet.Substring(0, halfLength);

                var excess = ret.Length - this.minHashLength;
                if (excess > 0)
                    ret = ret.Substring(excess / 2, this.minHashLength);
            }

            return ret.ToString();
		}

		private string Hash(int input, string alphabet)
		{
            var hash = string.Empty;

            do
            {
                hash = alphabet[input % alphabet.Length] + hash;
                input = (int)input / alphabet.Length;
            } while (input > 0);

            return hash;
		}

		private int Unhash(string input, string alphabet)
		{
            int number = 0;

            for (var i = 0; i < input.Length; i++)
            {
                var pos = alphabet.IndexOf(input[i]);
                number += (int)(pos * Math.Pow(alphabet.Length, input.Length - i - 1));
            }

            return number;
		}

		/// <summary>
		/// Decodes the provided hash
		/// </summary>
		/// <param name="hash"></param>
		/// <returns></returns>
		private int[] Decode(string hash, string alphabet)
		{
            var ret = new List<int>();
            int i = 0;

            var hashBreakdown = guardsRegex.Replace(hash, " ");
            var hashArray = hashBreakdown.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (hashArray.Length == 3 || hashArray.Length == 2)
                i = 1;

            hashBreakdown = hashArray[i];
            if (hashBreakdown[0] != default(char))
            {
                var lottery = hashBreakdown[0];
                hashBreakdown = hashBreakdown.Substring(1);

                hashBreakdown = sepsRegex.Replace(hashBreakdown, " ");
                hashArray = hashBreakdown.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                for(var j = 0; j < hashArray.Length; j++)
                {
                    var subHash = hashArray[j];
                    var buffer = lottery + this.salt + alphabet;

                    alphabet = ConsistentShuffle(alphabet, buffer.Substring(0, alphabet.Length));
                    ret.Add(Unhash(subHash, alphabet));
                }

                if (Encode(ret.ToArray()) != hash)
                    ret.Clear();
            }

            return ret.ToArray();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="alphabet"></param>
		/// <param name="salt"></param>
		/// <returns></returns>
		private string ConsistentShuffle(string alphabet, string salt)
		{
            if (string.IsNullOrWhiteSpace(salt))
                return alphabet;

            int v, p, n, j;
            v = p = n = j = 0;

            for (var i = alphabet.Length - 1; i > 0; i--, v++)
            {
                v %= salt.Length;
                p += n = (int)salt[v];
                j = (n + v + p) % i;

                var temp = alphabet[j];
                alphabet = alphabet.Substring(0, j) + alphabet[i] + alphabet.Substring(j + 1);
                alphabet = alphabet.Substring(0, i) + temp + alphabet.Substring(i + 1);
            }

            return alphabet;
		}
	} 
}
