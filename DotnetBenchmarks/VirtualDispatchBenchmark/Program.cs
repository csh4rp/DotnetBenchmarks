using System;
using BenchmarkDotNet.Running;

namespace VirtualDispatchBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Benchmark>();
        }
    }
}