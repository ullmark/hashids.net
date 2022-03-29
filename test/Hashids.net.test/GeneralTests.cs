﻿using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace HashidsNet.test
{
    public class GeneralTests
    {
        private const string salt = "this is my salt";
        private readonly Hashids _hashids = new Hashids(salt);

        [Fact]
        public async Task EncodingIsThreadSafe()
        {
            var hashids = new Hashids();
            const int threadCount = 6;
            const int numberCount = 1000001;

            var tasks = Enumerable.Range(1, threadCount).Select(t => Task.Run(() =>
            {
                for (var n = 1; n < numberCount; n++)
                {
                    var s = hashids.Encode(n);
                    hashids.Decode(s).Should().Equal(n);
                }
            })).ToArray();

            await Task.WhenAll(tasks);
        }

        [Fact]
        public void DefaultSaltIsBlank()
        {
            // default salt of empty string "" should result in this encoded value
            new Hashids().Encode(1, 2, 3).Should().Be("o2fXhV");
        }

        [Fact]
        public void SingleInt_Encodes()
        {
            _hashids.Encode(1).Should().Be("NV");
            _hashids.Encode(22).Should().Be("K4");
            _hashids.Encode(333).Should().Be("OqM");
            _hashids.Encode(9999).Should().Be("kQVg");
            _hashids.Encode(123000).Should().Be("58LzD");
            _hashids.Encode(456000000).Should().Be("5gn6mQP");
            _hashids.Encode(987654321).Should().Be("oyjYvry");
        }

        [Fact]
        public void SingleReturn_Decodes()
        {
            _hashids.DecodeSingle("NkK9").Should().Be(12345);
            _hashids.DecodeSingle("5O8yp5P").Should().Be(666555444);
            _hashids.DecodeSingle("Wzo").Should().Be(1337);
            _hashids.DecodeSingle("DbE").Should().Be(808);
            _hashids.DecodeSingle("yj8").Should().Be(303);

            Assert.Throws<MultipleResultsException>(() => _hashids.DecodeSingle("NkK9,NkK9").Should().Be(12345));
            Assert.Throws<MultipleResultsException>(() => _hashids.DecodeSingle("5O8yp5P,5O8yp5P").Should().Be(12345));
            Assert.Throws<MultipleResultsException>(() => _hashids.DecodeSingle("Wzo,Wzo").Should().Be(12345));
            Assert.Throws<MultipleResultsException>(() => _hashids.DecodeSingle("DbE,DbE").Should().Be(12345));
            Assert.Throws<MultipleResultsException>(() => _hashids.DecodeSingle("yj8,yj8").Should().Be(12345));
        }

        [Fact]
        public void SingleReturnOut_Decodes()
        {
            int value;

            _hashids.TryDecodeSingle("NkK9,NkK9", out value).Should().Be(false);
            _hashids.TryDecodeSingle("NkK9", out value).Should().Be(true);
            value.Should().Be(12345);

            _hashids.TryDecodeSingle("5O8yp5P,5O8yp5P", out value).Should().Be(false);
            _hashids.TryDecodeSingle("5O8yp5P", out value).Should().Be(true);
            value.Should().Be(666555444);

            _hashids.TryDecodeSingle("Wzo,Wzo", out value).Should().Be(false);
            _hashids.TryDecodeSingle("Wzo", out value).Should().Be(true);
            value.Should().Be(1337);

            _hashids.TryDecodeSingle("DbE,DbE", out value).Should().Be(false);
            _hashids.TryDecodeSingle("DbE", out value).Should().Be(true);
            value.Should().Be(808);

            _hashids.TryDecodeSingle("yj8,yj8", out value).Should().Be(false);
            _hashids.TryDecodeSingle("yj8", out value).Should().Be(true);
            value.Should().Be(303);
        }

        [Fact]
        public void SingleInt_Decodes()
        {
            _hashids.Decode("NkK9").Should().Equal(new[] { 12345 });
            _hashids.Decode("5O8yp5P").Should().Equal(new[] { 666555444 });
            _hashids.Decode("Wzo").Should().Equal(new[] { 1337 });
            _hashids.Decode("DbE").Should().Equal(new[] { 808 });
            _hashids.Decode("yj8").Should().Equal(new[] { 303 });
        }

        [Fact]
        public void SingleLong_Encodes()
        {
            _hashids.EncodeLong(1L).Should().Be("NV");
            _hashids.EncodeLong(2147483648L).Should().Be("21OjjRK");
            _hashids.EncodeLong(4294967296L).Should().Be("D54yen6");
            _hashids.EncodeLong(666555444333222L).Should().Be("KVO9yy1oO5j");
            _hashids.EncodeLong(12345678901112L).Should().Be("4bNP1L26r");
            _hashids.EncodeLong(Int64.MaxValue).Should().Be("jvNx4BjM5KYjv");
        }

        [Fact]
        public void SingleLong_Decode()
        {
            _hashids.DecodeLong("NV").Should().Equal(new[] { 1L });
            _hashids.DecodeLong("21OjjRK").Should().Equal(new[] { 2147483648L });
            _hashids.DecodeLong("D54yen6").Should().Equal(new[] { 4294967296L });
            _hashids.DecodeLong("KVO9yy1oO5j").Should().Equal(new[] { 666555444333222L });
            _hashids.DecodeLong("4bNP1L26r").Should().Equal(new[] { 12345678901112L });
            _hashids.DecodeLong("jvNx4BjM5KYjv").Should().Equal(new[] { Int64.MaxValue });
        }

        [Fact]
        public void SingleReturnLong_Decode()
        {
            _hashids.DecodeSingleLong("NV").Should().Be(1L);
            _hashids.DecodeSingleLong("21OjjRK").Should().Be(2147483648L);
            _hashids.DecodeSingleLong("D54yen6").Should().Be(4294967296L);
            _hashids.DecodeSingleLong("KVO9yy1oO5j").Should().Be(666555444333222L);
            _hashids.DecodeSingleLong("4bNP1L26r").Should().Be(12345678901112L);
            _hashids.DecodeSingleLong("jvNx4BjM5KYjv").Should().Be(Int64.MaxValue);

            Assert.Throws<MultipleResultsException>(() => _hashids.DecodeSingleLong("NV,NV").Should().Be(1L));
            Assert.Throws<MultipleResultsException>(() => _hashids.DecodeSingleLong("21OjjRK,21OjjRK").Should().Be(2147483648L));
            Assert.Throws<MultipleResultsException>(() => _hashids.DecodeSingleLong("D54yen6,D54yen6").Should().Be(4294967296L));
            Assert.Throws<MultipleResultsException>(() => _hashids.DecodeSingleLong("KVO9yy1oO5j,KVO9yy1oO5j").Should().Be(666555444333222L));
            Assert.Throws<MultipleResultsException>(() => _hashids.DecodeSingleLong("4bNP1L26r,4bNP1L26r").Should().Be(12345678901112L));
            Assert.Throws<MultipleResultsException>(() => _hashids.DecodeSingleLong("jvNx4BjM5KYjv,jvNx4BjM5KYjv").Should().Be(Int64.MaxValue));
        }

        [Fact]
        public void SingleReturnOutLong_Decodes()
        {
            long value;

            _hashids.TryDecodeSingleLong("NV,NV", out value).Should().Be(false);
            _hashids.TryDecodeSingleLong("NV", out value).Should().Be(true);
            value.Should().Be(1L);

            _hashids.TryDecodeSingleLong("21OjjRK,21OjjRK", out value).Should().Be(false);
            _hashids.TryDecodeSingleLong("21OjjRK", out value).Should().Be(true);
            value.Should().Be(2147483648L);

            _hashids.TryDecodeSingleLong("D54yen6,D54yen6", out value).Should().Be(false);
            _hashids.TryDecodeSingleLong("D54yen6", out value).Should().Be(true);
            value.Should().Be(4294967296L);

            _hashids.TryDecodeSingleLong("KVO9yy1oO5j,KVO9yy1oO5j", out value).Should().Be(false);
            _hashids.TryDecodeSingleLong("KVO9yy1oO5j", out value).Should().Be(true);
            value.Should().Be(666555444333222L);

            _hashids.TryDecodeSingleLong("4bNP1L26r,4bNP1L26r", out value).Should().Be(false);
            _hashids.TryDecodeSingleLong("4bNP1L26r", out value).Should().Be(true);
            value.Should().Be(12345678901112L);

            _hashids.TryDecodeSingleLong("jvNx4BjM5KYjv,jvNx4BjM5KYjv", out value).Should().Be(false);
            _hashids.TryDecodeSingleLong("jvNx4BjM5KYjv", out value).Should().Be(true);
            value.Should().Be(Int64.MaxValue);
        }

        [Fact]
        public void ListOfInt_Encodes()
        {
            _hashids.Encode(1, 2, 3).Should().Be("laHquq");
            _hashids.Encode(2, 4, 6).Should().Be("44uotN");
            _hashids.Encode(99, 25).Should().Be("97Jun");
            _hashids.Encode(1337, 42, 314).Should().Be("7xKhrUxm");
            _hashids.Encode(683, 94108, 123, 5).Should().Be("aBMswoO2UB3Sj");
            _hashids.Encode(547, 31, 241271, 311, 31397, 1129, 71129).Should().Be("3RoSDhelEyhxRsyWpCx5t1ZK");
            _hashids.Encode(21979508, 35563591, 57543099, 93106690, 150649789).Should().Be("p2xkL3CK33JjcrrZ8vsw4YRZueZX9k");
        }

        [Fact]
        public void ListOfInt_Decodes()
        {
            _hashids.Decode("1gRYUwKxBgiVuX").Should().Equal(new[] { 66655, 5444333, 2, 22 });
            _hashids.Decode("aBMswoO2UB3Sj").Should().Equal(new[] { 683, 94108, 123, 5 });
            _hashids.Decode("jYhp").Should().Equal(new[] { 3, 4 });
            _hashids.Decode("k9Ib").Should().Equal(new[] { 6, 5 });
            _hashids.Decode("EMhN").Should().Equal(new[] { 31, 41 });
            _hashids.Decode("glSgV").Should().Equal(new[] { 13, 89 });
        }

        [Fact]
        public void ListOfInt_Roundtrip()
        {
            var input = new[] { 12345, 67890, int.MaxValue };
            var decodedValue = _hashids.Decode(_hashids.Encode(input));
            decodedValue.Should().BeEquivalentTo(input);
        }

        [Fact]
        public void ListOfLong_Encodes()
        {
            _hashids.EncodeLong(666555444333222L, 12345678901112L).Should().Be("mPVbjj7yVMzCJL215n69");
        }

        [Fact]
        public void ListOfLong_Decodes()
        {
            _hashids.DecodeLong("mPVbjj7yVMzCJL215n69").Should().Equal(new[] { 666555444333222L, 12345678901112L });
        }

        [Fact]
        public void ListOfLong_Roundtrip()
        {
            var input = new[] { 1L, 1234567890123456789, long.MaxValue };
            var decodedValue = _hashids.DecodeLong(_hashids.EncodeLong(input));
            decodedValue.Should().BeEquivalentTo(input);
        }

        [Fact]
        public void HexString_Encode()
        {
            _hashids.EncodeHex("FA").Should().Be("lzY");
            _hashids.EncodeHex("26dd").Should().Be("MemE");
            _hashids.EncodeHex("FF1A").Should().Be("eBMrb");
            _hashids.EncodeHex("12abC").Should().Be("D9NPE");
            _hashids.EncodeHex("185b0").Should().Be("9OyNW");
            _hashids.EncodeHex("17b8d").Should().Be("MRWNE");
            _hashids.EncodeHex("1d7f21dd38").Should().Be("4o6Z7KqxE");
            _hashids.EncodeHex("20015111d").Should().Be("ooweQVNB");
        }

        [Fact]
        public void HexString_Decode()
        {
            _hashids.DecodeHex("lzY").Should().Be("FA");
            _hashids.DecodeHex("eBMrb").Should().Be("FF1A");
            _hashids.DecodeHex("D9NPE").Should().Be("12ABC");
        }

        [Fact]
        public void HexString_Roundtrip()
        {
            var hashids = new Hashids("this is my salt");

            var encoded = hashids.EncodeHex("DEADBEEF");
            encoded.Should().Be("kRNrpKlJ");

            var decoded = hashids.DecodeHex(encoded);
            decoded.Should().Be("DEADBEEF");

            var input2 = "1234567890ABCDEF";
            var decoded2 = hashids.DecodeHex(hashids.EncodeHex(input2));
            decoded2.Should().Be(input2);
        }

        [Fact]
        public void NumbersIncludingZero_AtStart_Roundtrip()
        {
            var input = new[] { 0, 1, 2 };
            var decodedValue = _hashids.Decode(_hashids.Encode(input));
            decodedValue.Should().Equal(input);
        }

        [Fact]
        public void NumbersIncludingZero_AtEnd_Roundtrip()
        {
            var input = new[] { 1, 2, 0 };
            var decodedValue = _hashids.Decode(_hashids.Encode(input));
            decodedValue.Should().Equal(input);
        }

        [Fact]
        public void ListOfIdenticalNumbers_DoNotProducePattern()
        {
            _hashids.Encode(5, 5, 5, 5).Should().Be("1Wc8cwcE");
        }

        [Fact]
        public void ListOfIncrementingNumbers_DoNotProducePattern()
        {
            _hashids.Encode(1, 2, 3, 4, 5, 6, 7, 8, 9, 10).Should().Be("kRHnurhptKcjIDTWC3sx");
        }

        [Fact]
        public void IncrementingNumbers_DoNotProduceSimilarPattern()
        {
            _hashids.Encode(1).Should().Be("NV");
            _hashids.Encode(2).Should().Be("6m");
            _hashids.Encode(3).Should().Be("yD");
            _hashids.Encode(4).Should().Be("2l");
            _hashids.Encode(5).Should().Be("rD");
        }

        [Fact]
        public void NoNumbers_ReturnsEmptyString()
        {
            _hashids.Encode().Should().Be(string.Empty);
            _hashids.EncodeLong().Should().Be(string.Empty);
        }

        [Fact]
        public void DecodeInvalidHexString_ReturnsEmptyString()
        {
            _hashids.EncodeHex("XYZ123").Should().Be(string.Empty);
        }

        [Fact]
        public void ListWithNegativeNumbers_ReturnsEmptyString()
        {
            _hashids.Encode(1, int.MaxValue, -3).Should().Be(string.Empty);
            _hashids.EncodeLong(1, long.MaxValue, -4).Should().Be(string.Empty);
        }

        [Fact]
        public void DifferentSalt_ReturnsEmptyList()
        {
            _hashids.Decode("NkK9").Should().Equal(new[] { 12345 });
            new Hashids("different salt").Decode("NkK9").Should().Equal(new int [0]);
        }

        [Fact]
        public void HashMinLength_EncodesHashWithAtLeastThatLength()
        {
            var hashLength = 18;
            var hashids = new Hashids(salt, hashLength);
            hashids.Encode(1).Length.Should().BeGreaterOrEqualTo(hashLength);
            hashids.Encode(4140, 21147, 115975, 678570, 4213597, 27644437).Length.Should().BeGreaterOrEqualTo(hashLength);
        }

        [Fact]
        public void HashMinLength_Decodes()
        {
            var hashids = new Hashids(salt, 8);
            hashids.Decode("gB0NV05e").Should().Equal(new[] { 1 });
            hashids.Decode("mxi8XH87").Should().Equal(new[] { 25, 100, 950 });
            hashids.Decode("KQcmkIW8hX").Should().Equal(new[] { 5, 200, 195, 1 });
        }

        [Fact]
        public void AlphabetContainsLessThan4UniqueChars_ThrowsArgumentException()
        {
            Action invocation = () => new Hashids(alphabet: "aadsss");
            invocation.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void AlphabetWithDashes_Roundtrip()
        {
            var hashids = new Hashids(alphabet: "abcdefghijklmnopqrstuvwxyz1234567890_-");
            var input = new long[] { 1, 2, 3 };
            var decodedValue = hashids.DecodeLong(hashids.EncodeLong(input));
            decodedValue.Should().BeEquivalentTo(input);
        }

        [Fact]
        public void AlphabetLessThanMinLength_ThrowsArgumentException()
        {
            var tooShortAlphabet = Hashids.DEFAULT_ALPHABET.Substring(0, Hashids.MIN_ALPHABET_LENGTH - 1);
            Action invocation = () => new Hashids(alphabet: tooShortAlphabet);
            invocation.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void AlphabetAtLeastMinLength_ShouldNotThrowException()
        {
            var minLengthAlphabet = Hashids.DEFAULT_ALPHABET.Substring(0, Hashids.MIN_ALPHABET_LENGTH);
            var largerLengthAlphabet = Hashids.DEFAULT_ALPHABET.Substring(0, Hashids.MIN_ALPHABET_LENGTH + 1);
            var results1 = new Hashids(alphabet: minLengthAlphabet);
            var results2 = new Hashids(alphabet: largerLengthAlphabet);
        }

        [Fact]
        public void NullAlphabet_ThrowsNullArgumentException()
        {
            Action invocation = () => new Hashids(alphabet: null);
            invocation.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CustomAlphabet_Roundtrip()
        {
            var hashids = new Hashids(salt, 0, "ABCDEFGhijklmn34567890-:");
            var input = new[] { 1, 2, 3, 4, 5 };
            hashids.Encode(input).Should().Be("6nhmFDikA0");
            hashids.Decode(hashids.Encode(input)).Should().BeEquivalentTo(input);
        }

        [Fact]
        public void CustomAlphabet2_Roundtrip()
        {
            var hashids = new Hashids(salt, 0, "ABCDEFGHIJKMNOPQRSTUVWXYZ23456789");
            var input = new[] { 1, 2, 3, 4, 5 };
            hashids.Encode(input).Should().Be("44HYIRU3TO");
            hashids.Decode(hashids.Encode(input)).Should().BeEquivalentTo(input);
        }

        [Fact]
        public void SaltIsLongerThanAlphabet_Roundtrip()
        {
            var longSalt = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            var hashids = new Hashids(salt: longSalt);
            var input = new[] { 1, 2, 0 };
            var decodedValue = hashids.Decode(hashids.Encode(input));
            decodedValue.Should().Equal(input);
        }

        [Fact]
        public void GuardCharacterOnly_DecodesToEmptyArray()
        {
            // no salt creates guard characters: "abde"
            var hashids = new Hashids("");
            var decodedValue = hashids.Decode("a");
            decodedValue.Should().Equal(Array.Empty<int>());
        }

        [Fact]
        public void PublicMethodsCanBeMocked()
        {
            var mock = new Mock<Hashids>();
            mock.Setup(hashids => hashids.Encode(It.IsAny<int[]>())).Returns("It works");
            mock.Object.Encode(new[] { 1 }).Should().Be("It works");
        }
    }
}
