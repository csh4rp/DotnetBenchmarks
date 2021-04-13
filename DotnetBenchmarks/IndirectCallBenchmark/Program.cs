using System;
using BenchmarkDotNet.Running;
using IndirectCallBenchmark.Models;

namespace IndirectCallBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Benchmark>();
        }
    }
}