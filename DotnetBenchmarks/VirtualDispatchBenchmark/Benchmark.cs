using BenchmarkDotNet.Attributes;
using VirtualDispatchBenchmark.Models;

namespace VirtualDispatchBenchmark
{
    public class Benchmark
    {
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
            _ = StaticCalculator.Add(X, Y);
        }

        [Benchmark(Description = "Direct call")]
        public void RunDirectCall()
        {
            _ = Calculator.Add(X, Y);
        }

        [Benchmark(Description = "Sealed child class call")]
        public void RunSealedChildClassCall()
        {
            _ = SealedCalculatorFromBaseClass.Add(X, Y);
        }

        [Benchmark(Description = "SubClass call")]
        public void RunSubClassCall()
        {
            _ = CalculatorFromBaseClass.Add(X, Y);
        }

        [Benchmark(Description = "Base class call")]
        public void RunBaseClass()
        {
            _ = BaseCalculator.Add(X, Y);
        }

        [Benchmark(Description = "Interface call")]
        public void RunInterface()
        {
            _ = InterfaceCalculator.Add(X, Y);
        }
    }
}