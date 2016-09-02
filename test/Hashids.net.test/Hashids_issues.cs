using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace HashidsNet.test
{
    public class Hashids_issues
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
        void issue_14_it_should_decode_encode_hex_correctly()
        {
            var hashids = new Hashids("this is my salt");
            var encoded = hashids.EncodeHex("DEADBEEF");
            encoded.Should().Be("kRNrpKlJ");

            var decoded = hashids.DecodeHex(encoded);
            decoded.Should().Be("DEADBEEF");

            var encoded2 = hashids.EncodeHex("1234567890ABCDEF");
            var decoded2 = hashids.DecodeHex(encoded2);
            decoded2.Should().Be("1234567890ABCDEF");
        }
    }
}
