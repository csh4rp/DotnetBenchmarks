using System.Diagnostics;
using BenchmarkDotNet.Attributes;

namespace CacheLineBenchmark
{
    public class Benchmark
    {
        private const int ArraySize = 1024;
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
            for (var i = 0; i < ItemsArray.Length; i++)
            {
                for (var j = 0; j < ArraySize; j++)
                {
                    _ = ItemsArray[i, j];
                }
            }
        }

        [Benchmark(Description = "By rows")]
        public void RunByRows()
        {
            for (var i = 0; i < ArraySize; i++)
            {
                for (var j = 0; j < ItemsArray.Length; j++)
                {
                    _ = ItemsArray[j, i];
                }
            }
        }
    }
}