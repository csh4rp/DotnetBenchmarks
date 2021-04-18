using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BenchmarkDotNet.Attributes;
using LinqBenchmark.Models;

namespace LinqBenchmark
{
    [MemoryDiagnoser]
    public class Benchmark
    {
        private static List<string> _result;
        
        public static IEnumerable<object[]> TestData()
        {
            yield return new object[] {PersonRange(100), 50, 60};
            yield return new object[] {PersonRange(1000), 500, 600};
            yield return new object[] {PersonRange(10000), 5000, 6000};
        }

        private static List<Person> PersonRange(int numberOfItems)
        {
            var result = new List<Person>(numberOfItems);
            for (var i = 0; i < numberOfItems; i++)
            {
                result.Add( new Person
                {
                    Id = i,
                    FirstName = $"First_Name_{i}",
                    LastName = $"Last_Name_{i}"
                });
            }

            return result;
        }

        [Benchmark(Description = "For loop", Baseline = true)]
        [ArgumentsSource(nameof(TestData))]
        public void RunForLoopSearch(List<Person> items, int minId, int maxId)
        {
            _result = new List<string>();
            for (var i = 0; i < items.Count; i++)
            {
                var person = items[i];
                if (person.Id >= minId && person.Id <= maxId)
                {
                    _result.Add(person.FirstName);
                }
            }
        }
        
        [Benchmark(Description = "Foreach loop")]
        [ArgumentsSource(nameof(TestData))]
        public void RunForEachLoopSearch(List<Person> items, int minId, int maxId)
        {
            _result = new List<string>();
            foreach (var person in items)
            {
                if (person.Id >= minId && person.Id <= maxId)
                {
                    _result.Add(person.FirstName);
                }
            }
        }
        
        [Benchmark(Description = "Linq")]
        [ArgumentsSource(nameof(TestData))]
        public void RunLinqSearch(List<Person> items, int minId, int maxId)
        {
            _result = items.Where(person => person.Id >= minId && person.Id <= maxId)
                .Select(person => person.FirstName)
                .ToList();
        }
    }
}