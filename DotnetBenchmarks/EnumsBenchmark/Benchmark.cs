using System;
using BenchmarkDotNet.Attributes;
using EnumsBenchmark.Models;

namespace EnumsBenchmark;

[MemoryDiagnoser]
public class Benchmark
{
    private static readonly Options Option = Options.Delete;
    private static readonly OptionsEnumeration OptionEnumeration = OptionsEnumeration.Delete;
    private static string _value;
    private static Options _option;
    private static OptionsEnumeration _optionsEnumeration;

    [Benchmark(Description = "Enum - ToString")]
    public void RunToStringForEnum()
    {
        _ = Option.ToString();
    }

    [Benchmark(Description = "Enumeration - ToString")]
    public void RunToStringForEnumeration()
    {
        _value = OptionEnumeration.ToString();
    }

    [Benchmark(Description = "Enum - Description")]
    public void RunDescriptionForEnum()
    {
        _value = Option.GetDescription();
    }

    [Benchmark(Description = "Enumeration - Description")]
    public void RunDescriptionForEnumeration()
    {
        _value = OptionEnumeration.Description;
    }

    [Benchmark(Description = "Enum - Parse")]
    public void RunParseForEnum()
    {
        _option = Enum.Parse<Options>("Write");
    }

    [Benchmark(Description = "Enumeration - Parse")]
    public void RunParseForEnumeration()
    {
        _optionsEnumeration = OptionsEnumeration.Parse("Write");
    }
}