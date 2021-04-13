using BenchmarkDotNet.Running;

namespace FalseSharingBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Benchmark>();
        }
    }
}