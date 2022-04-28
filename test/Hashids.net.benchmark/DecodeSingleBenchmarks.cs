using BenchmarkDotNet.Attributes;

namespace Hashids.net.benchmark
{
    public class DecodeSingleBenchmarks : ABBenchmarks
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
            VersionAInstance.DecodeSingle(_input);
        }

        [Benchmark]
        public override void VersionB()
        {
            VersionBInstance.DecodeSingle(_input);
        }

        [Benchmark]
        public override void VersionC()
        {
            VersionCInstance.DecodeSingle(_input);
        }

        [Params(5, 12345, int.MaxValue)]
        public int Value { get; set; }
    }
}
