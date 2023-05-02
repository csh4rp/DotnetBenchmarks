namespace VirtualDispatchBenchmark.Models;

public class CalculatorFromBaseClass : BaseCalculator
{
    public override int Add(int x, int y) => x + y;
}