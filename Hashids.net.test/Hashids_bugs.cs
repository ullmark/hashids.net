using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HashidsNet.test
{
    public class Hashids_bugs
    {
        [Fact]
        public void issue_8_should_not_throw_out_of_range_exception()
        {
            var hashids = new Hashids("janottaa", 6);
            var numbers = hashids.Decode("NgAzADEANAA=");
        }
    }
}
