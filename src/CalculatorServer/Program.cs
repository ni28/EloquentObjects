using System;
using EloquentObjects;

namespace ConsoleApplication1
{
    internal static class ServiceProgram
    {
        private static void Main(string[] args)
        {
            using (var remoteObjectServer = new EloquentServer("tcp://127.0.0.1:50000"))
            {
                remoteObjectServer.Add<IEloquentCalculator>("endpoint1", new EloquentCalculator
                {
                    Name = "qwetrt"
                });
                Console.ReadLine();
            }
        }
    }
}