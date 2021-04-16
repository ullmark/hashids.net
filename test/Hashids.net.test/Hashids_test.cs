using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Moq;
using Xunit;

namespace HashidsNet.test
{
    public class Hashids_test
    {
        private const string salt = "this is my salt";
        private const string defaultAlphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        private const string defaultSeps = "cfhistuCFHISTU";

        private readonly Hashids hashids;

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
            new Hashids().Encode(1,2,3).Should().Be("o2fXhV");
        }

        [Fact]
        void it_encodes_a_single_number()
        {
            hashids.Encode(1).Should().Be("NV");
            hashids.Encode(22).Should().Be("K4");
            hashids.Encode(333).Should().Be("OqM");
            hashids.Encode(9999).Should().Be("kQVg");
            hashids.Encode(123000).Should().Be("58LzD");
            hashids.Encode(456000000).Should().Be("5gn6mQP");
            hashids.Encode(987654321).Should().Be("oyjYvry");

        }

        [Fact]
        void it_encodes_a_single_long()
        {
            hashids.EncodeLong(1L).Should().Be("NV");
            hashids.EncodeLong(2147483648L).Should().Be("21OjjRK");
            hashids.EncodeLong(4294967296L).Should().Be("D54yen6");

            hashids.EncodeLong(666555444333222L).Should().Be("KVO9yy1oO5j");
            hashids.EncodeLong(12345678901112L).Should().Be("4bNP1L26r");
            hashids.EncodeLong(Int64.MaxValue).Should().Be("jvNx4BjM5KYjv");
        }

        [Fact]
        void it_encodes_a_list_of_numbers()
        {
            hashids.Encode(1,2,3).Should().Be("laHquq");
            hashids.Encode(2,4,6).Should().Be("44uotN");
            hashids.Encode(99,25).Should().Be("97Jun");

            hashids.Encode(1337,42,314).
              Should().Be("7xKhrUxm");

            hashids.Encode(683, 94108, 123, 5).
              Should().Be("aBMswoO2UB3Sj");

            hashids.Encode(547, 31, 241271, 311, 31397, 1129, 71129).
              Should().Be("3RoSDhelEyhxRsyWpCx5t1ZK");

            hashids.Encode(21979508, 35563591, 57543099, 93106690, 150649789).
              Should().Be("p2xkL3CK33JjcrrZ8vsw4YRZueZX9k");
        }

        [Fact]
        void it_encodes_a_list_of_longs()
        {
            hashids.EncodeLong(666555444333222L, 12345678901112L).Should().Be("mPVbjj7yVMzCJL215n69");
        }

        [Fact]
        void it_returns_an_empty_string_if_no_numbers()
        {
            hashids.Encode().Should().Be(string.Empty);
        }

        [Fact]
        void it_can_encodes_to_a_minimum_length()
        {
            var h = new Hashids(salt, 18);
            h.Encode(1).Should().Be("aJEDngB0NV05ev1WwP");

            h.Encode(4140, 21147, 115975, 678570, 4213597, 27644437).
                Should().Be("pLMlCWnJSXr1BSpKgqUwbJ7oimr7l6");
        }

        [Fact]
        void it_can_encode_with_a_custom_alphabet()
        {
            var h = new Hashids(salt, 0, "ABCDEFGhijklmn34567890-:");
            h.Encode(1, 2, 3, 4, 5).Should().Be("6nhmFDikA0");
        }

        [Fact]
        void it_does_not_produce_repeating_patterns_for_identical_numbers()
        {
            hashids.Encode(5, 5, 5, 5).Should().Be("1Wc8cwcE");
        }

        [Fact]
        void it_does_not_produce_repeating_patterns_for_incremented_numbers()
        {
            hashids.Encode(1, 2, 3, 4, 5, 6, 7, 8, 9, 10).
                Should().Be("kRHnurhptKcjIDTWC3sx");
        }

        [Fact]
        void it_does_not_produce_similarities_between_incrementing_number_hashes()
        {
            hashids.Encode(1).Should().Be("NV");
            hashids.Encode(2).Should().Be("6m");
            hashids.Encode(3).Should().Be("yD");
            hashids.Encode(4).Should().Be("2l");
            hashids.Encode(5).Should().Be("rD");
        }

        [Fact]
        void it_encode_hex_string()
        {
            hashids.EncodeHex("FA").Should().Be("lzY");
            hashids.EncodeHex("26dd").Should().Be("MemE");
            hashids.EncodeHex("FF1A").Should().Be("eBMrb");
            hashids.EncodeHex("12abC").Should().Be("D9NPE");
            hashids.EncodeHex("185b0").Should().Be("9OyNW");
            hashids.EncodeHex("17b8d").Should().Be("MRWNE");
            hashids.EncodeHex("1d7f21dd38").Should().Be("4o6Z7KqxE");
            hashids.EncodeHex("20015111d").Should().Be("ooweQVNB");
        }

        [Fact]
        void it_returns_an_empty_string_if_passed_non_hex_string()
        {
            hashids.EncodeHex("XYZ123").Should().Be(string.Empty);
        }

        [Fact]
        void it_decodes_an_encoded_number()
        {
            hashids.Decode("NkK9").Should().Equal(new [] { 12345 });
            hashids.Decode("5O8yp5P").Should().Equal(new [] { 666555444 });

            hashids.Decode("Wzo").Should().Equal(new [] { 1337 });
            hashids.Decode("DbE").Should().Equal(new [] { 808 });
            hashids.Decode("yj8").Should().Equal(new[] { 303 });
        }

        [Fact]
        void it_decodes_an_encoded_long()
        {
            hashids.DecodeLong("NV").Should().Equal(new[] { 1L });
            hashids.DecodeLong("21OjjRK").Should().Equal(new[] { 2147483648L });
            hashids.DecodeLong("D54yen6").Should().Equal(new[] { 4294967296L });

            hashids.DecodeLong("KVO9yy1oO5j").Should().Equal(new[] { 666555444333222L });
            hashids.DecodeLong("4bNP1L26r").Should().Equal(new[] { 12345678901112L });
            hashids.DecodeLong("jvNx4BjM5KYjv").Should().Equal(new[] { Int64.MaxValue });
        }

        [Fact]
        void it_decodes_a_list_of_encoded_numbers()
        {
            hashids.Decode("1gRYUwKxBgiVuX").Should().Equal(new [] { 66655,5444333,2,22 });
            hashids.Decode("aBMswoO2UB3Sj").Should().Equal(new [] { 683, 94108, 123, 5 });

            hashids.Decode("jYhp").Should().Equal(new [] { 3, 4 });
            hashids.Decode("k9Ib").Should().Equal(new [] { 6, 5 });

            hashids.Decode("EMhN").Should().Equal(new [] { 31, 41 });
            hashids.Decode("glSgV").Should().Equal(new[] { 13, 89 });
        }

        [Fact]
        void it_decodes_a_list_of_longs()
        {
            hashids.DecodeLong("mPVbjj7yVMzCJL215n69").Should().Equal(new[] { 666555444333222L, 12345678901112L });
        }

        [Fact]
        void it_does_not_decode_with_a_different_salt()
        {
            var peppers = new Hashids("this is my pepper");
            hashids.Decode("NkK9").Should().Equal(new []{ 12345 });
            peppers.Decode("NkK9").Should().Equal(new int [0]);
        }

        [Fact]
        void it_can_decode_from_a_hash_with_a_minimum_length()
        {
            var h = new Hashids(salt, 8);
            h.Decode("gB0NV05e").Should().Equal(new [] {1});
            h.Decode("mxi8XH87").Should().Equal(new[] { 25, 100, 950 });
            h.Decode("KQcmkIW8hX").Should().Equal(new[] { 5, 200, 195, 1 });
        }

        [Fact]
        void it_decode_an_encoded_number()
        {
            hashids.DecodeHex("lzY").Should().Be("FA");
            hashids.DecodeHex("eBMrb").Should().Be("FF1A");
            hashids.DecodeHex("D9NPE").Should().Be("12ABC");
        }

        [Fact]
        void it_raises_an_argument_null_exception_when_alphabet_is_null()
        {
            Action invocation = () => new Hashids(alphabet: null);
            invocation.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        void it_raises_an_argument_null_exception_if_alphabet_contains_less_than_4_unique_characters()
        {
            Action invocation = () => new Hashids(alphabet: "aadsss");
            invocation.Should().Throw<ArgumentException>();
        }

        [Fact]
        void it_encodes_and_decodes_numbers_starting_with_0()
        {
            var hash = hashids.Encode(0, 1, 2);
            hashids.Decode(hash).Should().Equal(new[] { 0, 1, 2 });
        }

        [Fact]
        void it_encodes_and_decodes_numbers_ending_with_0()
        {
            var hash = hashids.Encode(1, 2, 0);
            hashids.Decode(hash).Should().Equal(new[] { 1, 2, 0 });
        }

        [Fact]
        void our_public_methods_can_be_mocked()
        {
            var mock = new Mock<Hashids>();
            mock.Setup(hashids => hashids.Encode(It.IsAny<int[]>())).Returns("It works");
            mock.Object.Encode(new[] { 1 }).Should().Be("It works");
        }
    }
}
