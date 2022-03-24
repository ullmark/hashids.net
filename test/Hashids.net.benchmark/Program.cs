using BenchmarkDotNet.Running;

namespace Hashids.net.benchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<HashBenchmarks>();
        }
    }
}
