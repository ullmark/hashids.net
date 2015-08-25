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

        [Fact]
        void issue_12_should_not_throw_out_of_range_exception_when_downcasting()
        {
            var hashids = new Hashids();
            var random = new Random();
            for (var i = 0; i < 1000000; i++)
            {
                var l = (long)random.Next(1, Int32.MaxValue);
                var lo = l * random.Next(1, Int32.MaxValue);
                var hash = hashids.EncodeLong(lo);
                var val = hashids.DecodeLong(hash);
                val.First().Should().Be(lo);
            }
        }
    }
}
