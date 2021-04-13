using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace AsyncBenchmark
{
    public class Benchmark
    {
        private const int NumberOfIterations = 100_000_000;
        
        [Benchmark(Description = "With await")]
        public async Task RunWithAwait()
        {
            for (var i = 0; i < NumberOfIterations; i++)
            {
                await GetAwaitedTask();
            }
        }
        
        [Benchmark(Description = "Without await")]
        public async Task RunWithoutAwait()
        {
            for (var i = 0; i < NumberOfIterations; i++)
            {
                await GetTask();
            }
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
}