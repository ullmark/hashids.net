using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using HashidsNet;

namespace Hashids.net.benchmark
{
    [MemoryDiagnoser]
    [MemoryRandomization]
    [DisassemblyDiagnoser]
    [SimpleJob(RuntimeMoniker.Net48)]
    [SimpleJob(RuntimeMoniker.Net50)]
    [SimpleJob(RuntimeMoniker.Net60)]
    public abstract class ABBenchmarks
    {
        [GlobalSetup]
        public virtual void Setup()
        {
            VersionAInstance = new HashidsPrev(minHashLength: MinHashLength);
            VersionBInstance = new HashidsNet.Hashids(minHashLength: MinHashLength);
            VersionCInstance = new HashidsNet.Hashids(minHashLength: MinHashLength, useCache: false);
        }

        public abstract void VersionA();
        public abstract void VersionB();
        public abstract void VersionC();

        [Params(0, 18)]
        public int MinHashLength { get; set; }

        protected IHashids VersionAInstance { get; private set; }
        protected IHashids VersionBInstance { get; private set; }
        protected IHashids VersionCInstance { get; private set; }
    }
}
