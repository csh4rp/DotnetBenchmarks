using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Running;

namespace AsyncBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Benchmark>();
        }
    }
}