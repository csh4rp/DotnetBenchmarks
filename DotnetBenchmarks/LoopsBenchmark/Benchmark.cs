using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using LoopsBenchmark.Models;

namespace LoopsBenchmark
{
    [MemoryDiagnoser]
    public class Benchmark
    {
        public static IEnumerable<object[]> TestData()
        {
            yield return new object[] {Create(10)};
            yield return new object[] {Create(100)};
            yield return new object[] {Create(1000)};
            yield return new object[] {Create(10000)};
        }

        private static List<Person> Create(int numberOfItems)
        {
            var items = new List<Person>(numberOfItems);
            for (var i = 0; i < numberOfItems; i++)
            {
                items[i] = new Person {Id = i};
            }

            return items;
        }
        

        [Benchmark(Description = "For loop", Baseline = true)]
        [ArgumentsSource(nameof(TestData))]
        public void RunFor(List<Person> items)
        {
            for (var i = 0; i < items.Count; i++)
            {
                items[i].Id = 1;
            }
        }

        [Benchmark(Description = "List - Foreach loop")]
        [ArgumentsSource(nameof(TestData))]
        public void RunForEach(List<Person> items)
        {
            foreach (var person in items)
            {
                person.Id = 1;
            }
        }

        [Benchmark(Description = "List - foreach lambda loop")]
        [ArgumentsSource(nameof(TestData))]
        public void RunListForEach(List<Person> items)
        {
            items.ForEach(item => item.Id = 1);
        }

        [Benchmark(Description = "IEnumerable - foreach loop")]
        [ArgumentsSource(nameof(TestData))]
        public void RunIEnumerableForEach(IEnumerable<Person> items)
        {
            foreach (var person in items)
            {
                person.Id = 1;
            }
        }
    }
}