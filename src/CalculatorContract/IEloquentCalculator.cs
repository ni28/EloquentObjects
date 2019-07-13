using System;

namespace CalculatorContract
{
    public interface IEloquentCalculator
    {
        string Name { get; set; }

        int Add(int a, int b);
        
        void Sqrt(int a);

        IEloquentOperationsHistory OperationsHistory { get; }

        event EventHandler<OperationResult> ResultReady;
    }
}