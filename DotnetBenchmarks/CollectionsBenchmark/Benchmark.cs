using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace CollectionsBenchmark
{
    [MemoryDiagnoser]
    public class Benchmark
    {
        private const int NumberOfIterations = 10_000_000;
        private const int NumberOfItems = 100;

        private static readonly HashSet<int> ItemsSet = new(NumberOfIterations);
        private static readonly List<int> ItemsList = new(NumberOfIterations);
        private static readonly IEnumerable<int> ItemsEnumerable = ItemsList;

        static Benchmark()
        {
            for (var i = 0; i < NumberOfItems; i++)
            {
                ItemsSet.Add(i);
                ItemsList.Add(i);
            }
        }

        [Benchmark(Description = "Search for set")]
        public void RunSearchForSet()
        {
            var sum = 0L;
            for (var i = 0; i < NumberOfIterations; i++)
            {
                if (ItemsSet.Any() && ItemsSet.Contains(i))
                {
                    sum++;
                }
            }
        }
        
        [Benchmark(Description = "Search for list")]
        public void RunSearchForList()
        {
            var sum = 0L;
            for (var i = 0; i < NumberOfIterations; i++)
            {
                if (ItemsList.Any() && ItemsList.Contains(i))
                {
                    sum++;
                }
            }
        }

        [Benchmark(Description = "Search for enumerable")]
        public void RunSearchForEnumerable()
        {
            var sum = 0L;
            for (var i = 0; i < NumberOfIterations; i++)
            {
                if (ItemsEnumerable.Any() && ItemsEnumerable.Contains(i))
                {
                    sum++;
                }
            }
        }
        
        [Benchmark(Description = "Search for enumerable - materialized")]
        public void RunSearchForEnumerableMaterialized()
        {
            var sum = 0L;
            for (var i = 0; i < NumberOfIterations; i++)
            {
                var materialized = ItemsEnumerable.ToList();
                if (materialized.Any() && materialized.Contains(i))
                {
                    sum++;
                }
            }
        }
    }
}