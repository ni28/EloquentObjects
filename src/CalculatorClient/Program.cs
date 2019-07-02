using System;
using System.Linq;
using ConsoleApplication1;
using EloquentObjects;

namespace Client
{
    // State object for receiving data from remote device.  

    internal class Program
    {
        public static void Main(string[] args)
        {
            using (var client = new EloquentClient("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001"))
            using (var client2 = new EloquentClient("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50002"))
            {
                using (var calcObjConnection = client.Connect<IEloquentCalculator>("endpoint1"))
                {
                    calcObjConnection.Object.ResultReady += ObjectOnResultReady;
                    foreach (var i in Enumerable.Range(0, 2))
                    {
                        calcObjConnection.Object.Sqrt(i);
                    }
                    Console.WriteLine(calcObjConnection.Object.Add(1,4));
                    Console.WriteLine(calcObjConnection.Object.Name);
                    
                    
                    using (var objConnection = client2.Connect<IEloquentCalculator>("endpoint1"))
                    {
                        objConnection.Object.ResultReady += ObjectOnResultReady;
                        foreach (var i in Enumerable.Range(0, 2))
                        {
                            objConnection.Object.Sqrt(i);
                        }
                        Console.WriteLine(objConnection.Object.Add(1,4));
                        Console.WriteLine(objConnection.Object.Name);
                        objConnection.Object.Name = "1234";
                        Console.WriteLine(objConnection.Object.Name);
                        Console.ReadLine();
                        objConnection.Object.ResultReady -= ObjectOnResultReady;
                    }


                    Console.ReadLine();
                    calcObjConnection.Object.Name = "1234";
                    Console.WriteLine(calcObjConnection.Object.Name);
                    Console.ReadLine();
                    calcObjConnection.Object.ResultReady -= ObjectOnResultReady;

                }
                Console.ReadLine();
            }

            Console.ReadLine();

        }

        private static void ObjectOnResultReady(string id, OperationResult obj)
        {
            Console.WriteLine(id + ": " + obj.Value);
        }
    }

}