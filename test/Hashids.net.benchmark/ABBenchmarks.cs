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
        public void Setup()
        {
            VersionAInstance = new HashidsPrev();
            VersionBInstance = new HashidsNet.Hashids();
            VersionCInstance = new HashidsNet.Hashids(useCache: false);
        }

        public abstract void VersionA();
        public abstract void VersionB();
        public abstract void VersionC();

        protected IHashids VersionAInstance { get; private set; }
        protected IHashids VersionBInstance { get; private set; }
        protected IHashids VersionCInstance { get; private set; }
    }
}
