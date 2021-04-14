using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using LoopsBenchmark.Models;

namespace LoopsBenchmark
{
    [MemoryDiagnoser]
    public class Benchmark
    {
        private const int CollectionSize = 1000;
        private static readonly List<Person> Items = new(CollectionSize);
        private static readonly IEnumerable<Person> EnumerableItems = Items;
        private const int NumberOfIterations = 100_000_000;
        
        static Benchmark()
        {
            for (var i = 0; i < Items.Count; i++)
            {
                Items[i] = new Person {Id = i};
            }
        }
        
        [Benchmark(Description = "For loop", Baseline = true)]
        public void RunFor()
        {
            for (var i = 0; i < NumberOfIterations; i++)
            {
                for (var j = 0; j < Items.Count; j++)
                {
                    Items[j].Id = 1;
                }
            }
        }
        
        [Benchmark(Description = "List - Foreach loop")]
        public void RunForEach()
        {
            for (var i = 0; i < NumberOfIterations; i++)
            {
                foreach (var person in Items)
                {
                    person.Id = 1;
                }
            }
        }
        
        [Benchmark(Description = "List - foreach lambda loop")]
        public void RunListForEach()
        {
            for (var i = 0; i < NumberOfIterations; i++)
            {
                Items.ForEach(item => item.Id = 1);
            }
        }
        
        [Benchmark(Description = "IEnumerable - foreach loop")]
        public void RunIEnumerableForEach()
        {
            for (var i = 0; i < NumberOfIterations; i++)
            {
                foreach (var person in EnumerableItems)
                {
                    person.Id = 1;
                }
            }
        }
    }
}