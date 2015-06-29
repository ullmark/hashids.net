using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HashidsNet.test
{
    public class Hashids_perf
    {
        const int NUMBER_OF_ITERATIONS = 10000;
        Hashids hashids;

        public Hashids_perf()
        {
            hashids = new Hashids();
        }

        [Fact]
        void Test_encode_performance()
        {
            var time = TimeFor(() =>
            {
                for (var i = 1; i <= NUMBER_OF_ITERATIONS; i++)
                {
                    hashids.Encode(i); 
                }
            });

            Console.WriteLine("Encode, Single Number, {0} times => {1}ms", NUMBER_OF_ITERATIONS, time);
        }

        static TimeSpan TimeFor(Action action)
        {
            var stopWatch = Stopwatch.StartNew();
            action();
            stopWatch.Stop();
            return stopWatch.Elapsed;
        }
    }
}
