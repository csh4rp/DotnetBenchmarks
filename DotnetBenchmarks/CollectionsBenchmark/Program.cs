using System;
using BenchmarkDotNet.Running;

namespace CollectionsBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Benchmark>();
        }
    }
}