using System;
using EloquentObjects;

namespace ConsoleApplication1
{
    [EloquentInterface]
    public interface IEloquentCalculator
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