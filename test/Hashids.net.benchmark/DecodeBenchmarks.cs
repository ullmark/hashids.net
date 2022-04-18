using BenchmarkDotNet.Attributes;

namespace Hashids.net.benchmark
{
    public class DecodeBenchmarks : ABBenchmarks
    {
        private string _input;

        public override void Setup()
        {
            base.Setup();

            _input = VersionAInstance.Encode(Value);
        }

        [Benchmark(Baseline = true)]
        public override void VersionA()
        {
            VersionAInstance.Decode(_input);
        }

        [Benchmark]
        public override void VersionB()
        {
            VersionBInstance.Decode(_input);
        }

        [Benchmark]
        public override void VersionC()
        {
            VersionCInstance.Decode(_input);
        }

        [Params(
            new[] { 5 },
            new[] { 5, 12345 },
            new[] { 5, 12345, int.MaxValue }
        )]
        public int[] Value { get; set; }
    }
}
