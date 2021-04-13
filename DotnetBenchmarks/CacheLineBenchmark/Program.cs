using System;
using BenchmarkDotNet.Running;

namespace CacheLineBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Benchmark>();
        }
    }
}