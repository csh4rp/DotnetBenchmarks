using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace AsyncBenchmark
{
    public class Benchmark
    {
        [Benchmark(Description = "With await")]
        public async Task RunWithAwait()
        {
            await GetAwaitedTask();
        }
        
        [Benchmark(Description = "Without await")]
        public async Task RunWithoutAwait()
        {
            await GetTask();
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