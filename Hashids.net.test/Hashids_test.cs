using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Xunit;

namespace HashidsNet.test
{
	public class Hashids_test
	{
		Hashids hashids;
		private string salt = "this is my salt";
        private string defaultAlphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        private string defaultSeps = "cfhistuCFHISTU";

		public Hashids_test()
		{
			hashids = new Hashids(salt);
		}

        [Fact]
        void it_has_correct_default_alphabet()
        {
            Hashids.DEFAULT_ALPHABET.Should().Be(defaultAlphabet);
        }

        [Fact]
        void it_has_correct_default_separators()
        {
            Hashids.DEFAULT_SEPS.Should().Be(defaultSeps);
        }

		[Fact]
		void it_has_a_default_salt()
		{
			new Hashids().Encrypt(1,2,3).Should().Be("o2fXhV");
		}

		[Fact]
		void it_encrypts_a_single_number()
		{
            hashids.Encrypt(1).Should().Be("NV");
            hashids.Encrypt(22).Should().Be("K4");
            hashids.Encrypt(333).Should().Be("OqM");
            hashids.Encrypt(9999).Should().Be("kQVg");
            hashids.Encrypt(123000).Should().Be("58LzD");
            hashids.Encrypt(456000000).Should().Be("5gn6mQP");
            hashids.Encrypt(987654321).Should().Be("oyjYvry");
		}

		[Fact]
		void it_encrypts_a_list_of_numbers()
		{
            hashids.Encrypt(1,2,3).Should().Be("laHquq");
            hashids.Encrypt(2,4,6).Should().Be("44uotN");
            hashids.Encrypt(99,25).Should().Be("97Jun");

            hashids.Encrypt(1337,42,314).
              Should().Be("7xKhrUxm");

            hashids.Encrypt(683, 94108, 123, 5).
              Should().Be("aBMswoO2UB3Sj");

            hashids.Encrypt(547, 31, 241271, 311, 31397, 1129, 71129).
              Should().Be("3RoSDhelEyhxRsyWpCx5t1ZK");

            hashids.Encrypt(21979508, 35563591, 57543099, 93106690, 150649789).
              Should().Be("p2xkL3CK33JjcrrZ8vsw4YRZueZX9k");
		}

		[Fact]
		void it_returns_an_empty_string_if_no_numbers()
		{
			hashids.Encrypt().Should().Be(string.Empty);
		}

		[Fact]
		void it_can_encrypt_to_a_minimum_length()
		{
			var h = new Hashids(salt, 18);
            h.Encrypt(1).Should().Be("aJEDngB0NV05ev1WwP");

            h.Encrypt(4140, 21147, 115975, 678570, 4213597, 27644437).
                Should().Be("pLMlCWnJSXr1BSpKgqUwbJ7oimr7l6");
		}

        [Fact]
        void it_can_encrypt_with_a_custom_alphabet()
        {
            var h = new Hashids(salt, 0, "ABCDEFGhijklmn34567890-:");
            h.Encrypt(1, 2, 3, 4, 5).Should().Be("6nhmFDikA0");
        }

		[Fact]
		void it_does_not_produce_repeating_patterns_for_identical_numbers()
		{
            hashids.Encrypt(5, 5, 5, 5).Should().Be("1Wc8cwcE");
		}

        [Fact]
        void it_does_not_produce_repeating_patterns_for_incremented_numbers()
        {
            hashids.Encrypt(1, 2, 3, 4, 5, 6, 7, 8, 9, 10).
                Should().Be("kRHnurhptKcjIDTWC3sx");
        }

		[Fact]
		void it_does_not_produce_similarities_between_incrementing_number_hashes()
		{
            hashids.Encrypt(1).Should().Be("NV");
            hashids.Encrypt(2).Should().Be("6m");
            hashids.Encrypt(3).Should().Be("yD");
            hashids.Encrypt(4).Should().Be("2l");
            hashids.Encrypt(5).Should().Be("rD");
		}

        [Fact]
        void it_encrypts_hex_string()
        {
            hashids.EncryptHex("FA").Should().Be("lzY");
            hashids.EncryptHex("26dd").Should().Be("MemE");
            hashids.EncryptHex("FF1A").Should().Be("eBMrb");
            hashids.EncryptHex("12abC").Should().Be("D9NPE");
            hashids.EncryptHex("185b0").Should().Be("9OyNW");
            hashids.EncryptHex("17b8d").Should().Be("MRWNE");

            // TODO: Support long?
            //hashids.EncryptHex("1d7f21dd38").Should().Be("4o6Z7KqxE");
            //hashids.EncryptHex("20015111d").Should().Be("ooweQVNB");
        }

        [Fact]
        void it_returns_an_empty_string_if_passed_non_hex_string()
        {
            hashids.EncryptHex("XYZ123").Should().Be(string.Empty);
        }

		[Fact]
		void it_decrypts_an_ecrypted_number()
		{
            hashids.Decrypt("NkK9").Should().Equal(new [] { 12345 });
            hashids.Decrypt("5O8yp5P").Should().Equal(new [] { 666555444 });

            // TODO: support longs?
            //hashids.Decrypt("KVO9yy1oO5j").Should().Equal(new[] { 666555444333222 });

            hashids.Decrypt("Wzo").Should().Equal(new [] { 1337 });
            hashids.Decrypt("DbE").Should().Equal(new [] { 808 });
            hashids.Decrypt("yj8").Should().Equal(new[] { 303 });

		}

		[Fact]
		void it_decrypts_a_list_of_encrypted_numbers()
		{
            hashids.Decrypt("1gRYUwKxBgiVuX").Should().Equal(new [] { 66655,5444333,2,22 });
            hashids.Decrypt("aBMswoO2UB3Sj").Should().Equal(new [] { 683, 94108, 123, 5 });

            hashids.Decrypt("jYhp").Should().Equal(new [] { 3, 4 });
            hashids.Decrypt("k9Ib").Should().Equal(new [] { 6, 5 });

            hashids.Decrypt("EMhN").Should().Equal(new [] { 31, 41 });
            hashids.Decrypt("glSgV").Should().Equal(new[] { 13, 89 });
		}

		[Fact]
		void it_does_not_decrypt_with_a_different_salt()
		{
			var peppers = new Hashids("this is my pepper");
			hashids.Decrypt("NkK9").Should().Equal(new []{ 12345 });
			peppers.Decrypt("NkK9").Should().Equal(new int [0]);
		}

		[Fact]
		void it_can_decrypt_from_a_hash_with_a_minimum_length()
		{
			var h = new Hashids(salt, 8);
			h.Decrypt("gB0NV05e").Should().Equal(new [] {1});
            h.Decrypt("mxi8XH87").Should().Equal(new[] { 25, 100, 950 });
            h.Decrypt("KQcmkIW8hX").Should().Equal(new[] { 5, 200, 195, 1 });
		}

        [Fact]
        void it_decrypts_an_encrypted_number()
        {
            hashids.DecryptHex("lzY").Should().Be("FA");
            hashids.DecryptHex("eBMrb").Should().Be("FF1A");
            hashids.DecryptHex("D9NPE").Should().Be("12ABC");
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
        void it_encrypts_and_decrypts_numbers_starting_with_0()
        {
            var hash = hashids.Encrypt(0, 1, 2);
            hashids.Decrypt(hash).Should().Equal(new[] { 0, 1, 2 });
        }

        [Fact]
        void it_encrypts_and_decrypts_numbers_ending_with_0()
        {
            var hash = hashids.Encrypt(1, 2, 0);
            hashids.Decrypt(hash).Should().Equal(new[] { 1, 2, 0 });
        }
	}
}
