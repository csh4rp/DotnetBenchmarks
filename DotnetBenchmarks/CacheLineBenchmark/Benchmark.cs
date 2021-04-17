using System.Diagnostics;
using BenchmarkDotNet.Attributes;

namespace CacheLineBenchmark
{
    public class Benchmark
    {
        private const int ArraySize = 1024;
        private const int NumberOfIterations = 1000;
        private static readonly int[,] ItemsArray = new int[ArraySize, ArraySize];

        static Benchmark()
        {
            for (var i = 0; i < ArraySize; i++)
            {
                for (var j = 0; j < ArraySize; j++)
                {
                    ItemsArray[i, j] = 1;
                }
            }
        }

        [Benchmark(Description = "By columns")]
        public void RunByColumns()
        {
            for (var iteration = 0; iteration < NumberOfIterations; iteration++)
            {
                var sum = 0L;
                for (var i = 0; i < ItemsArray.Length; i++)
                {
                    for (var j = 0; j < ArraySize; j++)
                    {
                        sum += ItemsArray[i, j];
                    }
                }
            
                Debug.Assert(sum != 0);
            }
        }
        
        [Benchmark(Description = "By rows")]
        public void RunByRows()
        {
            for (var iteration = 0; iteration < NumberOfIterations; iteration++)
            {
                var sum = 0L;
                for (var i = 0; i < ArraySize; i++)
                {
                    for (var j = 0; j < ItemsArray.Length; j++)
                    {
                        sum += ItemsArray[j, i];
                    }
                }
            
                Debug.Assert(sum != 0);
            }
        }
    }
}