using System;
using BenchmarkDotNet.Running;

namespace LoopsBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Benchmark>();
        }
    }
}