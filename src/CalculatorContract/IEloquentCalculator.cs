using System;
using EloquentObjects;

namespace CalculatorContract
{
    [EloquentContract]
    public interface IEloquentCalculator
    {
        [EloquentProperty]
        string Name { get; set; }

        [EloquentMethod]
        int Add(int a, int b);
        
        [EloquentMethod(IsOneWay = true)]
        void Sqrt(int a);

        [EloquentProperty]
        IEloquentOperationsHistory OperationsHistory { get; }

        [EloquentEvent]
        event EventHandler<OperationResult> ResultReady;
    }
}