using System;
using System.Threading;

namespace ConsoleApplication1.DomainModel
{
    internal sealed class Calculator: ICalculator
    {
        public Calculator(IOperationsHistory operationsHistory)
        {
            OperationsHistory = operationsHistory;
        }

        #region Implementation of ICalculator

        public IOperationsHistory OperationsHistory { get; }

        public int Add(int a, int b)
        {
            var result = a + b;

            OperationsHistory.AddOperation($"{a} + {b} = {result}");
            
            return result;
        }

        public double Sqrt(double a)
        {
            //Simulate long running operation
            Thread.Sleep(1000);
            var result = Math.Sqrt(a);
            
            OperationsHistory.AddOperation($"sqrt({a}) = {result}");

            return result;
        }

        #endregion
    }
}