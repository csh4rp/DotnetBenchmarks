using System;
using BenchmarkDotNet.Attributes;
using EnumsBenchmark.Models;

namespace EnumsBenchmark
{
    public class Benchmark
    {
        private const int NumberOfIterations = 1_000_000;
        private static readonly Options Option = Options.Delete;
        private static readonly OptionsEnumeration OptionEnumeration = OptionsEnumeration.Delete;

        [Benchmark(Description = "Enum - ToString")]
        public void RunToStringForEnum()
        {
            for (var i = 0; i < NumberOfIterations; i++)
            {
                var str = Option.ToString();
            }
        }

        [Benchmark(Description = "Enumeration - ToString")]
        public void RunToStringForEnumeration()
        {
            for (var i = 0; i < NumberOfIterations; i++)
            {
                var str = OptionEnumeration.ToString();
            }
        }

        [Benchmark(Description = "Enum - Description")]
        public void RunDescriptionForEnum()
        {
            for (var i = 0; i < NumberOfIterations; i++)
            {
                var str = Option.GetDescription();
            }
        }

        [Benchmark(Description = "Enumeration - Description")]
        public void RunDescriptionForEnumeration()
        {
            for (var i = 0; i < NumberOfIterations; i++)
            {
                var str = OptionEnumeration.Description;
            }
        }

        [Benchmark(Description = "Enum - Parse")]
        public void RunParseForEnum()
        {
            for (var i = 0; i < NumberOfIterations; i++)
            {
                var str = Enum.Parse<Options>("Write");
            }
        }

        [Benchmark(Description = "Enumeration - Parse")]
        public void RunParseForEnumeration()
        {
            for (var i = 0; i < NumberOfIterations; i++)
            {
                var str = OptionsEnumeration.Parse("Write");
            }
        }
    }
}