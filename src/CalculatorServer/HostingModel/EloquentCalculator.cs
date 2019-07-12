using System;
using System.Threading;
using System.Threading.Tasks;
using CalculatorContract;
using EloquentObjects;

namespace ConsoleApplication1.HostingModel
{
    internal sealed class EloquentCalculator: IEloquentCalculator, IDisposable
    {
        private readonly ICalculator _calculator;
        private readonly IObjectHost<IEloquentCalculator> _objectHost;
        private readonly EloquentOperationsHistory _clientHost;

        public EloquentCalculator(ICalculator calculator, IEloquentServer server)
        {
            _calculator = calculator;

            _objectHost = server.Add<IEloquentCalculator>("calculator", this);

            _clientHost = new EloquentOperationsHistory(calculator.OperationsHistory, server);
        }
        
        #region Implementation of ICalculatorService

        public string Name { get; set; }

        public int Add(int a, int b)
        {
            Console.WriteLine($"Called Add({a}, {b}) on thread {Thread.CurrentThread.ManagedThreadId}");
            return _calculator.Add(a, b);
        }

        public void Sqrt(int a)
        {
            Task.Run(() =>
            {
                Console.WriteLine($"Called Sqrt({a}) on thread {Thread.CurrentThread.ManagedThreadId}");
                var result = _calculator.Sqrt(a);
                ResultReady?.Invoke(Thread.CurrentThread.ManagedThreadId.ToString(), new OperationResult(result));
            });
        }

        public IEloquentOperationsHistory OperationsHistory => _clientHost.ObjectHost.Object;

        public event EventHandler<OperationResult> ResultReady;

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _clientHost.Dispose();
            _objectHost.Dispose();
        }

        #endregion
    }
}