using System;
using ConsoleApplication1.DomainModel;
using ConsoleApplication1.HostingModel;
using EloquentObjects;

namespace ConsoleApplication1
{
    internal static class ServiceProgram
    {
        private static void Main()
        {
            var domainCalculator = new Calculator(new OperationsHistory());
            
            using (var remoteObjectServer = new EloquentServer("tcp://127.0.0.1:50000"))
            {
                using (var eloquentCalc = new EloquentCalculator(domainCalculator, remoteObjectServer))
                {
                    eloquentCalc.Name = "Calculator1";
                    Console.ReadLine();
                }
            }
        }
    }
}