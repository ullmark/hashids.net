using BenchmarkDotNet.Attributes;

namespace Hashids.net.benchmark
{
    public class DecodeSingleLongBenchmarks : ABBenchmarks
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
            VersionAInstance.DecodeSingleLong(_input);
        }

        [Benchmark]
        public override void VersionB()
        {
            VersionBInstance.DecodeSingleLong(_input);
        }

        [Benchmark]
        public override void VersionC()
        {
            VersionCInstance.DecodeSingleLong(_input);
        }

        [Params(5L, 12345L, 1234567890123456789L, long.MaxValue)]
        public long Value { get; set; }
    }
}
