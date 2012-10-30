using NSpec;
using System;

namespace Hashids.net.spec
{
	class describe_hashids : nspec
	{
		void describe_setup()
		{
			before = () => 
			{
				hashids = new Hashids("this is my salt");
				alphabet = "gdfogjdsq2sdf";
			};

			it["has a default salt"] = () => new Hashids().Salt.should_be("katKSA");
			it["has a default alphabet"] = () => hashids.Alphabet.should_be(Hashids.DEFAULT_ALPHABET);
			it["has the correct salt"] = () => hashids.Salt.should_be("this is my salt");
			it["has the correct alphabet"] = () => new Hashids(alphabet: alphabet).Alphabet.should_be(alphabet);
			it["raises an exception if alpabet is null or empty"] = expect<ArgumentNullException>(() => new Hashids(alphabet: null));
			it["raises an exception if the alphabet has less than 4 unique chars"] = expect<ArgumentException>(() => new Hashids(alphabet: "xyz"));
		}

		void describe_encrypt()
		{
			before = () => hashids = new Hashids(salt);
			it["encrypts a single number"] = () => 
			{
				hashids.Encrypt(12345).should_be("ryBo");
				hashids.Encrypt(1).should_be("LX");
				hashids.Encrypt(22).should_be("5B");
				hashids.Encrypt(333).should_be("o49");
				hashids.Encrypt(9999).should_be("GKnB");
			};
			it["can encrypt a list of numbers"] = () => 
			{
				hashids.Encrypt(683, 94108, 123, 5).should_be("zBphL54nuMyu5");
				hashids.Encrypt(1,2,3).should_be("eGtrS8");
				hashids.Encrypt(2,4,6).should_be("9Kh7fz");
				hashids.Encrypt(99,25).should_be("dAECX");
			};
			it["can encrypt to a minimum length"] = () => 
			{
				var h = new Hashids(salt, 8);
				h.Encrypt(1).should_be("b9iLXiAa");
			};
			it["can encrypt with a custom alphabet"] = () => 
			{
				var h = new Hashids(salt, 0, "abcd");
				h.Encrypt(1,2,3,4,5).should_be("adcdacddcdaacdad");
			};
		}

		void describe_decrypt()
		{
			it["decrypts an encrypted number"] = todo;
			it["decrypts a list of encrypted numbers"] = todo;
			it["does not decrypt with a different salt"] = todo;
			it["can decrypt from a hash with a minimum length"] = todo;
		}

		Hashids hashids;
		string alphabet;
		string salt = "this is my salt";
	}
}
