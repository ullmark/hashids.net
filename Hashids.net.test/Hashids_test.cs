using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Xunit;

namespace Hashids.net.test
{
	public class Hashids_test
	{
		Hashids hashids;
		private string salt = "this is my salt";

		public Hashids_test()
		{
			hashids = new Hashids(salt);
		}

		[Fact]
		void it_has_a_default_salt()
		{
			new Hashids().Encrypt(1,2,3).Should().Be("katKSA");
		}

		[Fact]
		void it_has_the_correct_salt()
		{
			hashids.Salt.Should().Be("this is my salt");
		}

		[Fact]
		void it_defaults_to_a_min_length_of_0()
		{
			hashids.MinHashLength.Should().Be(0);
		}

		[Fact]
		void it_encrypts_a_single_number()
		{
			hashids.Encrypt(12345).Should().Be("ryBo");
			hashids.Encrypt(1).Should().Be("LX");
			hashids.Encrypt(22).Should().Be("5B");
			hashids.Encrypt(333).Should().Be("o49");
			hashids.Encrypt(9999).Should().Be("GKnB");
		}

		[Fact]
		void it_encrypts_a_list_of_numbers()
		{
			hashids.Encrypt(683, 94108, 123, 5).Should().Be("zBphL54nuMyu5");
			hashids.Encrypt(1, 2, 3).Should().Be("eGtrS8");
			hashids.Encrypt(2, 4, 6).Should().Be("9Kh7fz");
			hashids.Encrypt(99, 25).Should().Be("dAECX");
		}

		[Fact]
		void it_returns_an_empty_string_if_no_numbers()
		{
			hashids.Encrypt().Should().Be(string.Empty);
		}

		[Fact]
		void it_can_encrypt_to_a_minimum_length()
		{
			hashids.Encrypt(1).Should().Equals("b9iLXiAa");
		}

		[Fact]
		void it_does_not_produce_repeating_patterns_for_identical_numbers()
		{
			hashids.Encrypt(5, 5, 5, 5).Should().Be("GLh5SMs9");
		}

		[Fact]
		void it_does_not_produce_repeating_patterns_for_incremented_numbers()
		{
			hashids.Encrypt(1, 2, 3, 4, 5, 6, 7, 8, 9, 10).Should().Be("zEUzfySGIpuyhpF6HaC7");
		}

		[Fact]
		void it_does_not_produce_similarities_between_incrementing_number_hashes()
		{
			hashids.Encrypt(1).Should().Be("LX");
			hashids.Encrypt(2).Should().Be("ed");
			hashids.Encrypt(3).Should().Be("o9");
			hashids.Encrypt(4).Should().Be("4n");
			hashids.Encrypt(5).Should().Be("a5");
		}

		[Fact]
		void it_decrypts_an_ecrypted_number()
		{
			hashids.Decrypt("ryBo").Should().Equal(new []{ 12345 });
			hashids.Decrypt("qkpA").Should().Equal(new [] { 1337 });
			hashids.Decrypt("6aX").Should().Equal(new [] { 808 });
			hashids.Decrypt("gz9").Should().Equal(new [] { 303 });
		}

		[Fact]
		void it_decrypts_a_list_of_encrypted_numbers()
		{
			hashids.Decrypt("zBphL54nuMyu5").Should().Equal(new[] { 683, 94108, 123, 5 });
			hashids.Decrypt("kEFy").Should().Equal(new[]{ 1, 2 });
			hashids.Decrypt("Aztn").Should().Equal(new[]{ 6, 5 });
		}

		[Fact]
		void it_does_not_decrypt_with_a_different_salt()
		{
			var peppers = new Hashids("this is my pepper");
			hashids.Decrypt("ryBo").Should().Equal(new []{ 12345 });
			peppers.Decrypt("ryBo").Should().Equal(new int [0]);
		}

		[Fact]
		void it_can_decrypt_from_a_hash_with_a_minimum_length()
		{
			var h = new Hashids(salt, 8);
			h.Decrypt("b9iLXiAa").Should().Equal(new [] {1});
		}

		[Fact]
		void it_raises_an_argument_null_exception_when_alphabet_is_null()
		{
			Action invocation = () => new Hashids(alphabet: null);
			invocation.ShouldThrow<ArgumentNullException>();
		}

		[Fact]
		void it_raises_an_argument_null_exception_if_alphabet_contains_less_than_4_unique_characters()
		{
			Action invocation = () => new Hashids(alphabet: "aadsss");
			invocation.ShouldThrow<ArgumentException>();
		}

		[Fact]
		void it_can_encrypt_with_a_swapped_custom()
		{
			var hashids = new Hashids("this is my salt", 0, "abcd");
			hashids.Encrypt(1, 2, 3, 4, 5).Should().Be("adcdacddcdaacdad");
		}
	}
}
