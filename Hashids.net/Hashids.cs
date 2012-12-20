using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;

namespace Hashids.net
{
	/// <summary>
	/// Generates Hash ids.
	/// </summary>
	public class Hashids
	{
		public const string DEFAULT_ALPHABET = "xcS4F6h89aUbideAI7tkynuopqrXCgTE5GBKHLMjfRsz";

		private static int[] primes = new int[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43 };
		private static int[] sepsIndices = new int[] { 0, 4, 8, 12 };

		private char[] seps;
		private char[] guards;

		/// <summary>
		/// Initializes a new Hashids encrypt-/decrypt-er
		/// </summary>
		/// <param name="salt"></param>
		/// <param name="alphabet"></param>
		public Hashids(string salt = "", int minHashLength = 0, string alphabet = DEFAULT_ALPHABET)
		{
			if (string.IsNullOrWhiteSpace(alphabet))
				throw new ArgumentNullException("alphabet");

			this.Salt = salt;
			this.Alphabet = string.Join(string.Empty, alphabet.Distinct());
			this.MinHashLength = minHashLength;

			if (this.Alphabet.Length < 4)
				throw new ArgumentException("alphabet must contain atleast 4 unique characters.", "alphabet");
			
			this.SetupAlphabet();
		}

		/// <summary>
		/// Prepares the alphabet for encypting/decryping
		/// </summary>
		private void SetupAlphabet()
		{
			var seps = new List<char>();
			var guards = new List<char>();

			foreach (var prime in primes)
			{
				var c = Alphabet.ElementAtOrDefault(prime - 1);
				if (c != default(char))
				{
					seps.Add(c);
					Alphabet = Alphabet.Replace(c, ' ');
				}
			}

			foreach (var index in sepsIndices)
			{
				var separator = seps.ElementAtOrDefault(index);
				if (separator != default(char))
				{
					guards.Add(separator);
					seps.RemoveAt(index);
				}
			}

			Alphabet = Alphabet.Replace(" ", string.Empty);
			Alphabet = ConsistentShuffle(Alphabet, Salt);
			
			this.seps = seps.ToArray();
			this.guards = guards.ToArray();
		}

		/// <summary>
		/// Encrypts the provided numbers into a hash.
		/// </summary>
		/// <param name="number">the numbers</param>
		/// <returns>the hash</returns>
		public string Encrypt(params int[] numbers)
		{
			return Encode(numbers, Alphabet, Salt, MinHashLength);
		}

		/// <summary>
		/// Decrypts the provided numbers into a array of numbers
		/// </summary>
		/// <param name="hash">hash</param>
		/// <returns>array of numbers.</returns>
		public int[] Decrypt(string hash)
		{
			return Decode(hash);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="numbers"></param>
		/// <param name="alphabet"></param>
		/// <param name="salt"></param>
		/// <param name="minHashLength"></param>
		/// <returns></returns>
		private string Encode(int[] numbers, string alphabet, string salt, int minHashLength = 0)
		{
			var ret = new StringBuilder();

			var seps = ConsistentShuffle(string.Join(string.Empty, this.seps), string.Join("", numbers)).ToArray();
			char lotteryChar = default(char);

			for (var i = 0; i < numbers.Length; i++)
			{
				if (i == 0)
				{
					var lotterySalt = string.Join("-", numbers);
					foreach (var number in numbers) { lotterySalt += string.Concat("-", (number + 1) * 2); }

					var lottery = ConsistentShuffle(alphabet, lotterySalt);
					lotteryChar = lottery[0];
					ret.Append(lotteryChar);
					
					alphabet = string.Concat(lotteryChar, alphabet.Replace(lotteryChar.ToString(), string.Empty));
				}

				alphabet = ConsistentShuffle(alphabet, string.Concat((int)lotteryChar & 12345, salt));
				ret.Append(Hash(numbers[i], alphabet));

				if ((i + 1) < numbers.Length)
				{
					var sepsIndex = (numbers[i] + i) % seps.Length;
					ret.Append(seps[sepsIndex]);
				}
			}

			if (ret.Length < minHashLength)
			{
				var firstIndex = 0;
				for (var i = 0; i < numbers.Length; i++)
				{
					firstIndex += (i + 1) * numbers[i];
				}
				
				var guardIndex = firstIndex % guards.Length;
				var guard = guards[guardIndex];
				ret.Insert(0, guard);

				if (ret.Length < minHashLength)
				{
					guardIndex = (guardIndex + ret.Length) % guards.Length;
					guard = guards[guardIndex];

					ret.Append(guard);
				}
			}

			while (ret.Length < minHashLength)
			{
				var padArray = new [] {(int)alphabet[1], (int)alphabet[0]};
				var padLeft = Encode(padArray, alphabet, salt);
				var padRight = Encode(padArray, alphabet, string.Join(string.Empty, padArray));

				ret.Insert(0, padLeft);
				ret.Append(padRight);

				var excess = ret.Length - minHashLength;
				var r = ret.ToString();

				if (excess > 0)
				{
					ret.Clear();
					ret.Append(r.Substring(excess / 2, minHashLength));
				}
				
				alphabet = ConsistentShuffle(alphabet, salt + ret.ToString());
			}

			return ret.ToString();
		}

		private string Hash(int number, string alphabet)
		{
			var hash = string.Empty;

			while (number > 0)
			{
				hash = string.Concat(alphabet[number % alphabet.Length], hash);
				number = number / alphabet.Length;
			}

			return hash;
		}

		private int Unhash(string hash, string alphabet)
		{
			var number = 0;
			for (var i = 0; i < hash.Length; i++)
			{
				var pos = alphabet.IndexOf(hash[i]);
				number += pos * (int)Math.Pow(alphabet.Length, hash.Length - i - 1);
			}

			return number;
		}

		/// <summary>
		/// Decodes the provided hash
		/// </summary>
		/// <param name="hash"></param>
		/// <returns></returns>
		private int[] Decode(string hash)
		{
			var ret = new List<int>();
			var originalHash = hash;
			
			if (!string.IsNullOrEmpty(hash))
			{
				var alphabet = "";
				var lotteryChar = default(char);
				var i = 0;

				for (i = 0; i < guards.Length; i++)
					hash = hash.Replace(guards[i], ' ');

				var hashSplit = hash.Split(' ');
				i = 0;

				if (hashSplit.Length == 3 || hashSplit.Length == 2)
					i = 1;

				hash = hashSplit[i];

				for (i = 0; i < seps.Length; i++)
					hash = hash.Replace(seps[i], ' ');

				var hashArray = hash.Split(' ');

				for (i = 0; i < hashArray.Length; i++)
				{
					var subHash = hashArray[i];
					if (!string.IsNullOrEmpty(subHash))
					{
						if (i == 0)
						{
							lotteryChar = hash[0];
							subHash = subHash.Substring(1);
							alphabet = lotteryChar + Alphabet.Replace(lotteryChar.ToString(), string.Empty);
						}

						if (alphabet.Length > 0)
						{
							alphabet = ConsistentShuffle(alphabet, string.Concat((int)lotteryChar & 12345, this.Salt));
							ret.Add(Unhash(subHash, alphabet));
						}
					}

					//
				}
			}

			var numbers = ret.ToArray();
			if (Encrypt(numbers) != originalHash)
			{
				return new int[0];
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
			var ret = new StringBuilder();

			if (string.IsNullOrEmpty(salt))
				salt = new string(new [] {(char)0});

			var alphabetList = alphabet.ToList();
			var sortingArray = salt.Select(c => (int)c).ToArray();

			for (var i = 0; i < sortingArray.Length; i++)
			{
				var add = true;
				var k = i;

				while (k != (sortingArray.Length + i - 1))
				{
					var nextIndex = (k + 1) % sortingArray.Length;

					if (add) sortingArray[i] += sortingArray[nextIndex] + (k * i);
					else sortingArray[i] -= sortingArray[nextIndex];
					
					add = !add;
					k += 1;
				}

				sortingArray[i] = Math.Abs(sortingArray[i]);
			}

			var j = 0;

			while (alphabetList.Count > 0)
			{
				var pos = sortingArray[j];
				if (pos >= alphabetList.Count)
				{ 
					pos %= alphabetList.Count;
				}
				ret.Append(alphabetList[pos]);
				alphabetList.RemoveAt(pos);
				j = ++j % sortingArray.Length;
			}

			return ret.ToString();
		}

		/// <summary>
		/// Gets the minimum hash length
		/// </summary>
		public int MinHashLength { get; private set; }

		/// <summary>
		/// Gets the Salt
		/// </summary>
		public string Salt { get; private set; }

		/// <summary>
		/// Gets the Alphabet
		/// </summary>
		public string Alphabet { get; private set; }
	} 
}
