using BenchmarkDotNet.Attributes;
using VirtualDispatchBenchmark.Models;

namespace VirtualDispatchBenchmark;

public class Benchmark
{
    private const int NumberOfIterations = 1000;
    private const int X = 1;
    private const int Y = 1;
    private static readonly ICalculator InterfaceCalculator = new CalculatorFromBaseClass();
    private static readonly BaseCalculator BaseCalculator = new CalculatorFromBaseClass();
    private static readonly CalculatorFromBaseClass CalculatorFromBaseClass = new();
    private static readonly SealedCalculatorFromBaseClass SealedCalculatorFromBaseClass = new();
    private static readonly Calculator Calculator = new();
    private static int _value;

    [Benchmark(Description = "Static call", Baseline = true)]
    public void RunStaticCall()
    {
        for (var i = 0; i < NumberOfIterations; i++) _value = StaticCalculator.Add(X, Y);
    }

    [Benchmark(Description = "Direct call")]
    public void RunDirectCall()
    {
        for (var i = 0; i < NumberOfIterations; i++) _value = Calculator.Add(X, Y);
    }

    [Benchmark(Description = "Sealed Subclass call")]
    public void RunSealedChildClassCall()
    {
        for (var i = 0; i < NumberOfIterations; i++) _value = SealedCalculatorFromBaseClass.Add(X, Y);
    }

    [Benchmark(Description = "SubClass call")]
    public void RunSubClassCall()
    {
        for (var i = 0; i < NumberOfIterations; i++) _value = CalculatorFromBaseClass.Add(X, Y);
    }

    [Benchmark(Description = "BaseClass call")]
    public void RunBaseClass()
    {
        for (var i = 0; i < NumberOfIterations; i++) _value = BaseCalculator.Add(X, Y);
    }

    [Benchmark(Description = "Interface call")]
    public void RunInterface()
    {
        for (var i = 0; i < NumberOfIterations; i++) _value = InterfaceCalculator.Add(X, Y);
    }
}