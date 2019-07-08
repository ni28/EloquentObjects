using System.Collections.Generic;

namespace ConsoleApplication1
{
    internal interface IOperationsHistory
    {
        IEnumerable<string> LastOperations { get; }

        void AddOperation(string operation);

        void Clear();
    }
}