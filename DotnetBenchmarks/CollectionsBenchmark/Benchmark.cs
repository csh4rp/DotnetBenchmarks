using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
namespace CollectionsBenchmark
{
    [MemoryDiagnoser]
    public class Benchmark
    {
        public IEnumerable<object[]> EnumerableItemsToFind()
        {
            yield return new object[]{ Enumerable.Range(0, 5).OrderBy(x => x % 3).ToArray(),
                Enumerable.Range(0, 10).OrderBy(x => x % 2)};
            yield return new object[]{ Enumerable.Range(0, 50).OrderBy(x => x % 3).ToArray(),
                Enumerable.Range(0, 100).OrderBy(x => x % 2)};
            yield return new object[]{ Enumerable.Range(0, 500).OrderBy(x => x % 3).ToArray(),
                Enumerable.Range(0, 1000).OrderBy(x => x % 2)};
        }
        
        public IEnumerable<object[]> SetItemsToFind()
        {
            yield return new object[]{ Enumerable.Range(0, 5).OrderBy(x => x % 3).ToArray(),
                Enumerable.Range(0, 10).OrderBy(x => x % 2).ToHashSet()};
            yield return new object[]{ Enumerable.Range(0, 50).OrderBy(x => x % 3).ToArray(),
                Enumerable.Range(0, 100).OrderBy(x => x % 2).ToHashSet()};
            yield return new object[]{ Enumerable.Range(0, 500).OrderBy(x => x % 3).ToArray(),
                Enumerable.Range(0, 1000).OrderBy(x => x % 2).ToHashSet()};
        }
        
        public IEnumerable<object[]> ListItemsToFind()
        {
            yield return new object[]{ Enumerable.Range(0, 5).OrderBy(x => x % 3).ToArray(), 
                Enumerable.Range(0, 10).OrderBy(x => x % 2).ToList()};
            yield return new object[]{ Enumerable.Range(0, 50).OrderBy(x => x % 3).ToArray(),
                Enumerable.Range(0, 100).OrderBy(x => x % 2).ToList()};
            yield return new object[]{ Enumerable.Range(0, 500).OrderBy(x => x % 3).ToArray(),
                Enumerable.Range(0, 1000).OrderBy(x => x % 2).ToList()};
        }

        [Benchmark(Description = "Set - not created")]
        [ArgumentsSource(nameof(EnumerableItemsToFind))]
        public void RunSearchForSet(int[] itemsToFind, IEnumerable<int> items)
        {
            var sum = 0;
            var set = items.ToHashSet();
            for (var j = 0; j < itemsToFind.Length; j++)
            {
                if (set.Contains(itemsToFind[j]))
                {
                    sum++;
                }
            }
        }
        
        [Benchmark(Description = "Set - pre created")]
        [ArgumentsSource(nameof(SetItemsToFind))]
        public void RunSearchForPreCreatedSet(int[] itemsToFind, HashSet<int> items)
        {
            var sum = 0;
            for (var j = 0; j < itemsToFind.Length; j++)
            {
                if (items.Contains(itemsToFind[j]))
                {
                    sum++;
                }
            }
        }
        
        [Benchmark(Description = "List - not created")]
        [ArgumentsSource(nameof(EnumerableItemsToFind))]
        public void RunSearchForPreCreatedList(int[] itemsToFind, IEnumerable<int> items)
        {
            var sum = 0;
            var list = items.ToList();
            for (var j = 0; j < itemsToFind.Length; j++)
            {
                if (list.Contains(itemsToFind[j]))
                {
                    sum++;
                }
            }
        }

        [Benchmark(Description = "List - pre created")]
        [ArgumentsSource(nameof(ListItemsToFind))]
        public void RunSearchForList(int[] itemsToFind, List<int> items)
        {
            var sum = 0;
            for (var j = 0; j < itemsToFind.Length; j++)
            {
                if (items.Contains(itemsToFind[j]))
                {
                    sum++;
                }
            }
        }

        [Benchmark(Description = "Enumerable")]
        [ArgumentsSource(nameof(EnumerableItemsToFind))]
        public void RunSearchForEnumerable(int[] itemsToFind, IEnumerable<int> items)
        {
            var sum = 0;
            for (var j = 0; j < itemsToFind.Length; j++)
            {
                if (items.Contains(itemsToFind[j]))
                {
                    sum++;
                }
            }
        }
    }
}