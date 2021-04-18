using System;
using BenchmarkDotNet.Running;

namespace LinqBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Benchmark>();
        }
    }
}