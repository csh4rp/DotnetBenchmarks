namespace VirtualDispatchBenchmark.Models
{
    public class SealedCalculatorFromBaseClass : BaseCalculator
    {
        public sealed override int Add(int x, int y) => x + y;
    }
}