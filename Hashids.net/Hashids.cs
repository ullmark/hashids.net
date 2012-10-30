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
		public const string VERSION = "0.0.1";
		public const string DEFAULT_ALPHABET = "xcS4F6h89aUbideAI7tkynuopqrXCgTE5GBKHLMjfRsz";

		private static IEnumerable<int> primes = new int[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43 };
		private static IEnumerable<int> sepsIndices = new int[] { 0, 4, 8, 12 };
		private IEnumerable<char> seps;
		private IEnumerable<char> guards;

		/// <summary>
		/// Initializes a new Hashids encrypt-/decrypt-er
		/// </summary>
		/// <param name="salt"></param>
		/// <param name="alphabet"></param>
		public Hashids(string salt = "katKSA", uint minHashLength = 0, string alphabet = DEFAULT_ALPHABET)
		{
			if (string.IsNullOrWhiteSpace(alphabet))
				throw new ArgumentNullException("alphabet");

			this.Salt = salt;
			this.Alphabet = string.Join(string.Empty, alphabet.Distinct());
			this.MinHashLength = minHashLength;

			if (alphabet.Length < 4)
				throw new ArgumentException("alphabet must contain atleast 4 unique characters.", "alphabet");
			
			seps = DetermineSeps();
			guards = DetermineGuards();
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="number"></param>
		/// <returns></returns>
		public string Encrypt(params uint[] numbers)
		{
			return Encode(numbers, Alphabet, Salt, MinHashLength);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hash"></param>
		/// <returns></returns>
		public uint[] Decrypt(string hash)
		{
			return null;
		}

		private string Encode(IEnumerable<uint> numbers, string alphabet, string salt, uint minHashLength)
		{
			var seps = ConsistentShuffle(alphabet, salt);
			return string.Empty;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private IEnumerable<char> DetermineSeps()
		{
			foreach (var prime in primes)
			{
				var sep = Alphabet.ElementAtOrDefault(prime - 1);
				if (sep == default(char)) break;
				Alphabet = Alphabet.Replace(sep, ' ');
				yield return sep;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private IEnumerable<char> DetermineGuards()
		{
			foreach (var indice in sepsIndices)
			{
				var separator = seps.ElementAtOrDefault(indice);
				if (separator != default(char))
				{
					seps = seps.Where((sep, index) => index != indice);
					yield return separator;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="alphabet"></param>
		/// <param name="salt"></param>
		/// <returns></returns>
		private IEnumerable ConsistentShuffle(string alphabet, string salt)
		{
			var ret = new StringBuilder();
			var alphabetList = alphabet.ToList();
			var sorting = salt.Select(c => (int)c).ToArray();

			for (var i = 0; i < sorting.Length; i++)
			{
				var add = true;
				var k = i;

				while (k != (sorting.Length + i - 1))
				{
					var nextIndex = (k + 1) % sorting.Length;

					if (add) sorting[i] += sorting[nextIndex] + (k * i);
					else sorting[i] -= sorting[nextIndex];
					
					add = !add;
					k += 1;
				}

				sorting[i] = Math.Abs(sorting[i]);
			}

			var j = 0;

			while (alphabetList.Count > 0)
			{
				var pos = sorting[j];
				if (pos >= alphabetList.Count) pos %= alphabetList.Count;
				ret.Append(alphabetList[pos]);
				alphabetList.RemoveAt(pos);
				j = (j + 1) % alphabetList.Count;
			}

			return ret.ToString();
		}

		public uint MinHashLength { get; private set; }
		public string Salt { get; private set; }
		public string Alphabet { get; private set; }
	} 
}
