using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace AsyncBenchmark;

public class Benchmark
{
    [Benchmark(Description = "Without await", Baseline = true)]
    public async Task RunWithoutAwait()
    {
        await GetTask();
    }

    [Benchmark(Description = "With await")]
    public async Task RunWithAwait()
    {
        await GetAwaitedTask();
    }

    private static async Task GetAwaitedTask()
    {
        await Task.CompletedTask;
    }

    private static Task GetTask()
    {
        return Task.CompletedTask;
    }
}