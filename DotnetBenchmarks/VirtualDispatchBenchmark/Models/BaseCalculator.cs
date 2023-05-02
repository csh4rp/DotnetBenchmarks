namespace VirtualDispatchBenchmark.Models;

public abstract class BaseCalculator : ICalculator
{
    public virtual int Add(int x, int y) => x + y;
}