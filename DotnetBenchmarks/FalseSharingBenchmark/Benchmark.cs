﻿using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace FalseSharingBenchmark
{
    public class Benchmark
    {
        private const int ArraySize = 1024;
        private const int NumberOfIterations = 1_000_000;
        private const int NumberOfTasks = 4;
        private static readonly int[] Items = new int[ArraySize];

        [Benchmark(Description = "Run using different cache line", Baseline = true)]
        public void RunUsingDifferentCacheLine()
        {
            const int slice = ArraySize / NumberOfTasks;
            var threads = new Thread[NumberOfTasks];
            for (var t = 0; t < NumberOfTasks; t++)
            {
                threads[t] = new Thread(idx =>
                {
                    var i = (int) idx;
                    var startIndex = i * slice;
                    var endIndex = (i + 1) * slice;
                    for (var j = 0; j < NumberOfIterations; j++)
                    {
                        for (var k = startIndex; k < endIndex; k++)
                        {
                            Items[k] = k;
                        }
                    }
                });
            }

            for (var i = 0; i < NumberOfTasks; i++)
                threads[i].Start(i);
            
            for (var i = 0; i < NumberOfTasks; i++)
                threads[i].Join();
        }
        
        [Benchmark(Description = "Run using the same cache line")]
        public void RunUsingTheSameCacheLine()
        {
            var threads = new Thread[NumberOfTasks];
            for (var t = 0; t < NumberOfTasks; t++)
            {
                threads[t] = new Thread(idx =>
                {
                    var i = (int) idx;
                    for (var j = 0; j < NumberOfIterations; j++)
                    {
                        for (var k = i; k < ArraySize; k += NumberOfTasks)
                        {
                            Items[k] = k;
                        }
                    }
                });
            }

            for (var i = 0; i < NumberOfTasks; i++)
                threads[i].Start(i);
            
            for (var i = 0; i < NumberOfTasks; i++)
                threads[i].Join();
        }
    }
}