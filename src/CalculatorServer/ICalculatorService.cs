using System;
using System.ServiceModel;
using EloquentObjects;

namespace ConsoleApplication1
{
    [EloquentInterface]
    public interface ICalculatorService
    {
        [EloquentProperty]
        string Name { get; set; }

        [EloquentMethod]
        int Add(int a, int b);
        
        [EloquentMethod]
        void Sqrt(int a);

        [EloquentEvent]
        event Action<string, OperationResult> ResultReady;
    }
}