using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;

namespace HashidsNet.test
{
    public class IssueSpecificTests
    {
        [Fact]
        void issue_8_should_not_throw_out_of_range_exception()
        {
            var hashids = new Hashids("janottaa", 6);
            var numbers = hashids.Decode("NgAzADEANAA=");
        }

        // This issue came from downcasting to int at the wrong place,
        // seems to happen when you are encoding A LOT of longs at the same time.
        // see if it is possible to make this a faster test (or remove it since it is unlikely that it will reapper).
        [Fact]
        void issue_12_should_not_throw_out_of_range_exception()
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
        void issue_15_it_should_return_emtpy_array_when_decoding_characters_missing_in_alphabet()
        {
            var hashids = new Hashids(salt: "Salty stuff", alphabet: "qwerty1234!¤%&/()=", seps: "1234");
            var numbers = hashids.Decode("abcd");
            numbers.Length.Should().Be(0);

            var hashids2 = new Hashids();
            hashids.Decode("13-37").Length.Should().Be(0);
            hashids.DecodeLong("32323kldffd!").Length.Should().Be(0);

            var hashids3 = new Hashids(alphabet: "1234567890;:_!#¤%&/()=", seps: "!#¤%&/()=");
            hashids.Decode("asdfb").Length.Should().Be(0);
            hashids.DecodeLong("asdfgfdgdfgkj").Length.Should().Be(0);
        }

        [Fact]
        void issue_64_it_should_be_possible_to_encode_and_decode_long_max_value()
        {
            var hashids = new Hashids("salt", alphabet: "0123456789ABCDEF");
            var hash = hashids.EncodeLong(long.MaxValue);

            hash.Should().Be("58E9BDD9A7598254DA4E");

            var decoded = hashids.DecodeSingleLong(hash);

            decoded.Should().Be(long.MaxValue);
        }
    }
}