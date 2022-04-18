using BenchmarkDotNet.Attributes;

namespace Hashids.net.benchmark
{
    public class EncodeParamsBenchmarks : ABBenchmarks
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
            new[] { 5 },
            new[] { 5, 12345 },
            new[] { 5, 12345, int.MaxValue }
        )]
        public int[] Value { get; set; }
    }
}
