using BenchmarkDotNet.Attributes;

namespace Hashids.net.benchmark
{
    public class EncodeLongBenchmarks : ABBenchmarks
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

        [Params(12345L, 1234567890123456789L, long.MaxValue)]
        public long Value { get; set; }
    }
}
