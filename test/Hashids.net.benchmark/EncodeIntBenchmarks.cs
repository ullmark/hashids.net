using BenchmarkDotNet.Attributes;

namespace Hashids.net.benchmark
{
    public class EncodeIntBenchmarks : ABBenchmarks
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

        [Params(5, 12345, int.MaxValue)]
        public int Value { get; set; }
    }
}
