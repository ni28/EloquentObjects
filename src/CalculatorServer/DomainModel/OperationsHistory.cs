using System.Collections.Generic;

namespace ConsoleApplication1.DomainModel
{
    internal sealed class OperationsHistory : IOperationsHistory
    {
        private readonly List<string> _lastOperations = new List<string>();
        
        #region Implementation of IOperationsHistory

        public IEnumerable<string> LastOperations => _lastOperations;
        public void AddOperation(string operation)
        {
            _lastOperations.Add(operation);
        }

        public void Clear()
        {
            _lastOperations.Clear();
        }

        #endregion
    }
}