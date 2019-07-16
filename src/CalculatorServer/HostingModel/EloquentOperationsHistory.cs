using System;
using System.Linq;
using CalculatorContract;
using EloquentObjects;

namespace ConsoleApplication1.HostingModel
{
    internal sealed class EloquentOperationsHistory: IEloquentOperationsHistory, IDisposable
    {
        private readonly IOperationsHistory _operationsHistory;

        public EloquentOperationsHistory(IOperationsHistory operationsHistory, IEloquentServer server)
        {
            _operationsHistory = operationsHistory;
            ObjectHost = server.Add<IEloquentOperationsHistory>("calculator/history", this);
        }

        public IObjectHost<IEloquentOperationsHistory> ObjectHost { get; }

        #region Implementation of IEloquentOperationsHistory

        public string[] Entries => _operationsHistory.LastOperations.ToArray();

        public void Clear()
        {
            _operationsHistory.Clear();
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            ObjectHost.Dispose();
        }

        #endregion
    }
}