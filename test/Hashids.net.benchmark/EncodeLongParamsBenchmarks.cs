using BenchmarkDotNet.Attributes;

namespace Hashids.net.benchmark
{
    public class EncodeLongParamsBenchmarks : ABBenchmarks
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
            new[] { 12345L },
            new[] { 12345L, 1234567890123456789L, long.MaxValue }
        )]
        public long[] Value { get; set; }
    }
}
