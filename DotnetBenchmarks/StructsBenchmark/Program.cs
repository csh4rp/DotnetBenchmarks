using System;
using BenchmarkDotNet.Running;

namespace StructsBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Benchmark>();
        }
    }
}