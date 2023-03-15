using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;

namespace HashidsNet.test
{
    public class IssueSpecificTests
    {
        [Fact]
        void Issue_8_should_not_throw_out_of_range_exception()
        {
            var hashids = new Hashids("janottaa", 6);
            var numbers = hashids.Decode("NgAzADEANAA=");
        }

        // This issue came from downcasting to int at the wrong place,
        // seems to happen when you are encoding A LOT of longs at the same time.
        // see if it is possible to make this a faster test (or remove it since it is unlikely that it will reapper).
        [Fact]
        void Issue_12_should_not_throw_out_of_range_exception()
        {
            var hash = new Hashids("zXZVFf2N38uV");
            var longs = new List<long>();
            var rand = new Random();
            var valueBuffer = new byte[8];
            var randLong = 0L;
            for (var i = 0; i < 100000; i++)
            {
                rand.NextBytes(valueBuffer);
                randLong = BitConverter.ToInt64(valueBuffer, 0);
                longs.Add(Math.Abs(randLong));
            }

            var encoded = hash.EncodeLong(longs);
            var decoded = hash.DecodeLong(encoded);
            decoded.Should().Equal(longs.ToArray());
        }

        [Fact]
        void Issue_15_it_should_return_empty_array_when_decoding_characters_missing_in_alphabet()
        {
            var hashids = new Hashids(salt: "Salty stuff", alphabet: "qwerty1234!¤%&/()=", seps: "1234");
            var numbers = hashids.Decode("abcd");
            numbers.Length.Should().Be(0);

            var hashids2 = new Hashids();
            hashids2.Decode("13-37").Length.Should().Be(0);
            hashids2.DecodeLong("32323kldffd!").Length.Should().Be(0);

            var hashids3 = new Hashids(alphabet: "1234567890;:_!#¤%&/()=", seps: "!#¤%&/()=");
            hashids3.Decode("asdfb").Length.Should().Be(0);
            hashids3.DecodeLong("asdfgfdgdfgkj").Length.Should().Be(0);
        }

        [Fact]
        void Issue_64_long_max_value_with_min_alphabet_length()
        {
            var hashids = new Hashids("salt", alphabet: "0123456789ABCDEF");
            var hash = hashids.EncodeLong(long.MaxValue);

            hash.Should().Be("58E9BDD9A7598254DA4E");

            var decoded = hashids.DecodeSingleLong(hash);

            decoded.Should().Be(long.MaxValue);
        }

        [Fact]
        void Issue75_1CharacterHashShouldNotThrowException()
        {
            var hashids = new Hashids("salt");
            Assert.Throws<NoResultException>(() => hashids.DecodeSingle("a"));
        }

        [Fact]
        void Issue75_TooShortHashShouldNotThrowException()
        {
            var hashids = new Hashids("salt");
            Assert.Throws<NoResultException>(() => hashids.DecodeSingle("ab"));
        }

        [Fact]
        void Issue75_TooShortHashWithLargerHashLengthShouldNotThrowException()
        {
            var hashids = new Hashids("salt", 40);
            Assert.Throws<NoResultException>(() => hashids.DecodeSingle("ab"));
        }

        [Fact]
        void Issue85_hash_shorter_than_min_length_should_not_throw_exception()
        {
            Hashids hashids = new Hashids(salt: "Dqa2s3RJBYPHUzg&R5qkF3Z4HLaWp#A^kMc^DqKVmqag2tasQjhz-PSM23=4", minHashLength: 8);
            int[] numbers = hashids.Decode("5111111"); // Length = 7

            Assert.Empty(numbers);
        }
    }
}
