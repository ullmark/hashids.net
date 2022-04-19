using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using HashidsNet;
using System;

namespace Hashids.net.benchmark
{
    [MemoryDiagnoser]
    [MemoryRandomization]
    [DisassemblyDiagnoser]
    [SimpleJob(RuntimeMoniker.Net48)]
    [SimpleJob(RuntimeMoniker.Net50)]
    [SimpleJob(RuntimeMoniker.Net60)]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByJob)]
    public class HashBenchmarks
    {
        private readonly int[] _ints = { 12345, 1234567890, int.MaxValue };
        private readonly long[] _longs = { 12345, 1234567890123456789, long.MaxValue };
        private readonly string _hex = "507f1f77bcf86cd799439011";

        private IHashids _hashids;

        [GlobalSetup]
        public void Setup()
        {
            _hashids = CreateInstance();
        }

        [Benchmark]
        public void RoundtripInts()
        {
            var encodedValue = _hashids.Encode(_ints);
            var decodedValue = _hashids.Decode(encodedValue);
        }

        [Benchmark]
        public void RoundtripLongs()
        {
            var encodedValue = _hashids.EncodeLong(_longs);
            var decodedValue = _hashids.DecodeLong(encodedValue);
        }

        [Benchmark]
        public void RoundtripHex()
        {
            var encodedValue = _hashids.EncodeHex(_hex);
            var decodedValue = _hashids.DecodeHex(encodedValue);
        }

        [Benchmark]
        public void SingleNumber()
        {
            var encoded = _hashids.Encode(5);
            var encodeLong = _hashids.EncodeLong(5);
        }

        [Benchmark]
        public void SingleNumberAsParams()
        {
            var encoded = _hashids.Encode(new[] { 1 });
            var encodedLong = _hashids.EncodeLong(new[] { (long)1 });
        }

        private IHashids CreateInstance()
        {
            switch (Version)
            {
                case HashidsVersion.Prev: return new HashidsPrev();
                case HashidsVersion.Current: return new HashidsNet.Hashids();
                case HashidsVersion.WithoutCache: return new HashidsNet.Hashids(useCache: false);
                default: throw new InvalidOperationException();
            }
        }

        [Params(
            HashidsVersion.Prev,
            HashidsVersion.Current,
            HashidsVersion.WithoutCache)]
        public HashidsVersion Version { get; set; }
    }
}