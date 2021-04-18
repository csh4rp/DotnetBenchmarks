using System;
using BenchmarkDotNet.Running;

namespace StringsBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Benchmark>();
        }
    }
}