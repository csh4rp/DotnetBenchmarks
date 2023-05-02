namespace IndirectCallBenchmark.Models;

public class Calculator : ICalculator
{
    public int Add(int x, int y) => x + y;
}