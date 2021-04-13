using BenchmarkDotNet.Attributes;
using VirtualDispatchBenchmark.Models;

namespace VirtualDispatchBenchmark
{
    public class Benchmark
    {
        private const int NumberOfIterations = 100_000_000;
        private const int X = 1;
        private const int Y = 1;

        private static readonly ICalculator InterfaceCalculator = new CalculatorFromBaseClass();
        private static readonly BaseCalculator BaseCalculator = new CalculatorFromBaseClass();
        private static readonly CalculatorFromBaseClass CalculatorFromBaseClass = new();
        private static readonly SealedCalculatorFromBaseClass SealedCalculatorFromBaseClass = new();
        private static readonly Calculator Calculator = new();

        [Benchmark(Description = "Static call", Baseline = true)]
        public void RunStaticCall()
        {
            var sum = 0L;
            for (var i = 0; i < NumberOfIterations; i++)
            {
                sum += StaticCalculator.Add(X, Y);
            }
        }
        
        [Benchmark(Description = "Direct call")]
        public void RunDirectCall()
        {
            var sum = 0L;
            for (var i = 0; i < NumberOfIterations; i++)
            {
                sum += Calculator.Add(X, Y);
            }
        }
        
        [Benchmark(Description = "Sealed child class call")]
        public void RunSealedChildClassCall()
        {
            var sum = 0L;
            for (var i = 0; i < NumberOfIterations; i++)
            {
                sum += SealedCalculatorFromBaseClass.Add(X, Y);
            }
        }

        [Benchmark(Description = "Child class call")]
        public void RunChildClassCall()
        {
            var sum = 0L;
            for (var i = 0; i < NumberOfIterations; i++)
            {
                sum += CalculatorFromBaseClass.Add(X, Y);
            }
        }

        [Benchmark(Description = "Base class call")]
        public void RunBaseClass()
        {
            var sum = 0L;
            for (var i = 0; i < NumberOfIterations; i++)
            {
                sum += BaseCalculator.Add(X, Y);
            }
        }
        
        [Benchmark(Description = "Interface call")]
        public void RunInterface()
        {
            var sum = 0L;
            for (var i = 0; i < NumberOfIterations; i++)
            {
                sum += InterfaceCalculator.Add(X, Y);
            }
        }
    }
}