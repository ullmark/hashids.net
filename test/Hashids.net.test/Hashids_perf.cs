using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using System.Diagnostics;

namespace HashidsNet.test
{
    public class Hashids_perf
    {
        [Fact]
        public void EncodePerformance()
        {
            var hashids = new Hashids();
            var stopWatch = Stopwatch.StartNew();
            for (var i = 1; i < 10001; i++)
            {
                hashids.Encode(i);
            }
            stopWatch.Stop();
            Trace.WriteLine($"10 000 encodes: {stopWatch.ElapsedMilliseconds}");
        }

        [Fact]
        public async Task ThreadSafe()
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
    }
}
