using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    internal sealed class EloquentCalculator: IEloquentCalculator
    {
        #region Implementation of ICalculatorService

        public string Name { get; set; }

        public int Add(int a, int b)
        {
            Console.WriteLine($"Add: {Thread.CurrentThread.ManagedThreadId}");
            return a + b;
        }

        public void Sqrt(int a)
        {
            Task.Run(() =>
            {
                Thread.Sleep(500);
                var result = Math.Sqrt(a);
                Console.WriteLine($"Sqrt: {Thread.CurrentThread.ManagedThreadId}");
                ResultReady?.Invoke(Thread.CurrentThread.ManagedThreadId.ToString(), new OperationResult(result));
            });
        }

        public event Action<string, OperationResult> ResultReady;

        #endregion
    }
}