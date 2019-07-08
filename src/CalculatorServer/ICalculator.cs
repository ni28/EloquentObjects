namespace ConsoleApplication1
{
    /// <summary>
    /// Interface for domain calculator.
    /// </summary>
    internal interface ICalculator
    {
        IOperationsHistory OperationsHistory { get; }
        
        int Add(int a, int b);
        
        double Sqrt(double a);
    }
}