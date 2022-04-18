using BenchmarkDotNet.Attributes;

namespace Hashids.net.benchmark
{
    public class DecodeLongBenchmarks : ABBenchmarks
    {
        private string _input;

        public override void Setup()
        {
            base.Setup();

            _input = VersionAInstance.EncodeLong(Value);
        }

        [Benchmark(Baseline = true)]
        public override void VersionA()
        {
            VersionAInstance.DecodeLong(_input);
        }

        [Benchmark]
        public override void VersionB()
        {
            VersionBInstance.DecodeLong(_input);
        }

        [Benchmark]
        public override void VersionC()
        {
            VersionCInstance.DecodeLong(_input);
        }

        [Params(
            new[] { 5L },
            new[] { 5L, 12345L },
            new[] { 5L, 12345L, 1234567890123456789L, long.MaxValue }
        )]
        public long[] Value { get; set; }
    }
}
