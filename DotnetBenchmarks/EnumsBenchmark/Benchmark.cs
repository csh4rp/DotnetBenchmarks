using System;
using BenchmarkDotNet.Attributes;
using EnumsBenchmark.Models;

namespace EnumsBenchmark
{
    [MemoryDiagnoser]
    public class Benchmark
    {
        private static readonly Options Option = Options.Delete;
        private static readonly OptionsEnumeration OptionEnumeration = OptionsEnumeration.Delete;

        [Benchmark(Description = "Enum - ToString")]
        public void RunToStringForEnum()
        {
            _ = Option.ToString();
        }

        [Benchmark(Description = "Enumeration - ToString")]
        public void RunToStringForEnumeration()
        {
            _ = OptionEnumeration.ToString();
        }

        [Benchmark(Description = "Enum - Description")]
        public void RunDescriptionForEnum()
        {
            _ = Option.GetDescription();
        }

        [Benchmark(Description = "Enumeration - Description")]
        public void RunDescriptionForEnumeration()
        {
            _ = OptionEnumeration.Description;
        }

        [Benchmark(Description = "Enum - Parse")]
        public void RunParseForEnum()
        {
            _ = Enum.Parse<Options>("Write");
        }

        [Benchmark(Description = "Enumeration - Parse")]
        public void RunParseForEnumeration()
        {
            _ = OptionsEnumeration.Parse("Write");
        }
    }
}