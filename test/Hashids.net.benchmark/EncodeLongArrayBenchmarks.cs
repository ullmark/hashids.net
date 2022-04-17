using BenchmarkDotNet.Attributes;

namespace Hashids.net.benchmark
{
    public class EncodeLongArrayBenchmarks : ABBenchmarks
    {
        [Benchmark(Baseline = true)]
        public override void VersionA()
        {
            VersionAInstance.EncodeLong(Value);
        }

        [Benchmark]
        public override void VersionB()
        {
            VersionBInstance.EncodeLong(Value);
        }

        [Benchmark]
        public override void VersionC()
        {
            VersionCInstance.EncodeLong(Value);
        }

        [Params(
            new[] { 5L }, 
            new[] { 5L, 12345L },
            new[] { 5L, 12345L, 1234567890123456789L, long.MaxValue }
        )]
        public long[] Value { get; set; }
    }
}
