using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Hashids.net.benchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<HashBenchmark>();
        }

        [MemoryDiagnoser]
        public class HashBenchmark
        {
            private readonly HashidsNet.Hashids _hashids;
            private readonly int[] _input = { 12345, 1234567890, int.MaxValue };

            public HashBenchmark()
            {
                _hashids = new HashidsNet.Hashids();
            }

            [Benchmark]
            public void Run()
            {
                var encodedValue = _hashids.Encode(_input);
                var decodedValue = _hashids.Decode(encodedValue);
            }
        }
    }
}