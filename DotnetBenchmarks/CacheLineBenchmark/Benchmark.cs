using BenchmarkDotNet.Attributes;

namespace CacheLineBenchmark;

public class Benchmark
{
    private const int ArraySize = 1024;
    private static readonly int[,] Items = new int[ArraySize, ArraySize];
    private static int _value;

    static Benchmark()
    {
        for (var i = 0; i < ArraySize; i++)
        for (var j = 0; j < ArraySize; j++)
            Items[i, j] = 1;
    }

    [Benchmark(Description = "By columns", Baseline = true)]
    public void RunByColumns()
    {
        for (var i = 0; i < ArraySize; i++)
        for (var j = 0; j < ArraySize; j++)
            _value = Items[i, j];
    }

    [Benchmark(Description = "By rows")]
    public void RunByRows()
    {
        for (var i = 0; i < ArraySize; i++)
        for (var j = 0; j < ArraySize; j++)
            _value = Items[j, i];
    }
}