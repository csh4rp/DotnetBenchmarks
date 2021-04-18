using System;
using System.Collections.Generic;
using System.Diagnostics;
using BenchmarkDotNet.Attributes;

namespace StringsBenchmark
{
    [MemoryDiagnoser]
    public class Benchmark
    {
        private static string _str;

        public static IEnumerable<object[]> ConcatParameters()
        {
            yield return new object[] {"First", "Second"};
            yield return new object[] {"First", "Second", "Third"};
            yield return new object[] {"First", "Second", "Third", "Forth"};
            yield return new object[] {"First", "Second", "Third", "Forth", "Fifth"};
        }
        
        public static IEnumerable<object[]> FormatParameters()
        {
            yield return new object[] {"{0}{1}", new object[]{"First", "Second"}};
            yield return new object[] {"{0}{1}{2}", new object[]{"First", "Second", "Third"}};
            yield return new object[] {"{0}{1}{2}{3}", new object[]{"First", "Second", "Third", "Forth"}};
            yield return new object[] {"{0}{1}{2}{3}{4}", new object[]{"First", "Second", "Third", "Forth", "Fifth"}};
        }
        
        [Benchmark(Description = "String.Concat", Baseline = true)]
        [ArgumentsSource(nameof(ConcatParameters))]
        public void RunStringConcat(object[] parameters)
        {
            _str = string.Concat(parameters);
        }
        
        [Benchmark(Description = "String.Format")]
        [ArgumentsSource(nameof(FormatParameters))]
        public void RunStringFormat(string format, object[] parameters)
        {
            _str = string.Format(format, parameters);
        }
        
    }
}