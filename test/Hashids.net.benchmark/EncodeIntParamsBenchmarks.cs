using BenchmarkDotNet.Attributes;

namespace Hashids.net.benchmark
{
    public class EncodeIntParamsBenchmarks : ABBenchmarks
    {
        [Benchmark(Baseline = true)]
        public override void VersionA()
        {
            VersionAInstance.Encode(Value);
        }

        [Benchmark]
        public override void VersionB()
        {
            VersionBInstance.Encode(Value);
        }

        [Benchmark]
        public override void VersionC()
        {
            VersionCInstance.Encode(Value);
        }

        [Params(
            new[] { 12345 },
            new[] { 12345, int.MaxValue }
        )]
        public int[] Value { get; set; }
    }
}
