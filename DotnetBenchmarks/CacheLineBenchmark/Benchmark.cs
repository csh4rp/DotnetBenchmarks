using System.Diagnostics;
using BenchmarkDotNet.Attributes;

namespace CacheLineBenchmark
{
    public class Benchmark
    {
        private const int DefaultPlatformObjectSizeInBytes = 24;
        private const int DefaultIntArrayObjectSize = DefaultPlatformObjectSizeInBytes + 4;
        private const int IntSizeInBytes = 4;
        private const int CacheLineSizeInBytes = 64;
        private const int CacheLinesToOccupy = 10;
        private const int ArraySize = 1024;
        private const int NumberOfIterations = 1000;
        private static readonly int[][] ItemsArray;

        static Benchmark()
        {
            const int bytesToOccupy = CacheLinesToOccupy * CacheLineSizeInBytes;
            const int intsToDeclare = (bytesToOccupy - DefaultIntArrayObjectSize) / IntSizeInBytes;
            ItemsArray = new int[intsToDeclare][];

            for (var i = 0; i < intsToDeclare; i++)
            {
                ItemsArray[i] = new int[ArraySize];
                for (var j = 0; j < ArraySize; j++)
                {
                    ItemsArray[i][j] = 1;
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
                        sum += ItemsArray[i][j];
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
                        sum += ItemsArray[j][i];
                    }
                }
            
                Debug.Assert(sum != 0);
            }
        }
    }
}