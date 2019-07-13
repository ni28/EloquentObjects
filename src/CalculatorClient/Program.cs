﻿using System;
using CalculatorContract;
using EloquentObjects;

namespace Client
{
    // State object for receiving data from remote device.  

    internal class Program
    {
        public static void Main(string[] args)
        {
            using (var client = new EloquentClient("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001"))
            {
                var calculator = client.Connect<IEloquentCalculator>("calculator");

                //Property get
                Console.WriteLine($"Calculator name: {calculator.Name}");

                //Property set
                calculator.Name = "MyCalculator";
                Console.WriteLine($"New calculator name: {calculator.Name}");

                //Method call
                Console.WriteLine($"1 + 2 = {calculator.Add(1, 2)}");

                //Subscribe to event
                calculator.ResultReady += CalculatorOnResultReady;                
                Console.WriteLine($"Call one-way long running operation: ");
                calculator.Sqrt(4);
                
                
                Console.WriteLine("Press Enter to exit");
                Console.ReadLine();
            }
        }

        private static void CalculatorOnResultReady(object sender, OperationResult result)
        {
            Console.WriteLine($"Long running operation result: {result.Value}");

            //Accessing to sender
            var senderAsClient = (IEloquentCalculator) sender;

            //Get Last Operations
            var history = senderAsClient.OperationsHistory;

            Console.WriteLine("Last operations:");
            foreach (var entry in history.OperationsHistory)
            {
                Console.WriteLine($"\t{entry}");
            }

            Console.WriteLine();

            history.Clear();

            Console.WriteLine("Last operations after clear:");
            foreach (var entry in history.OperationsHistory)
            {
                Console.WriteLine($"\t{entry}");
            }

            Console.WriteLine($"Sender name = {senderAsClient.Name}");
        }
    }

}