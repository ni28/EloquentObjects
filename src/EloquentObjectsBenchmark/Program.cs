using System;
using EloquentObjectsBenchmark.EloquentObjects.Benchmarks;

namespace EloquentObjectsBenchmark
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            const int iterations = 1000;
            const int numberOfEventClients = 5;

            RunWcfBenchmarks(iterations, numberOfEventClients);

            RunEloquentObjectsBenchmarks(iterations, numberOfEventClients);

            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }

        private static void RunWcfBenchmarks(int iterations, int numberOfEventClients)
        {
            var benchmarks = new IBenchmark[]
            {
                new Wcf.Benchmarks.OneWayCalls("net.tcp", iterations),
                new Wcf.Benchmarks.OneWayCalls("net.pipe", iterations),
                
                new Wcf.Benchmarks.TwoWayCalls("net.tcp", iterations),
                new Wcf.Benchmarks.TwoWayCalls("net.pipe", iterations),
                
                new Wcf.Benchmarks.SumOfTwoIntegers("net.tcp", iterations), 
                new Wcf.Benchmarks.SumOfTwoIntegers("net.pipe", iterations)
            };

            foreach (var benchmark in benchmarks)
            {
                var result = benchmark.Measure();
                Console.WriteLine(result);
            }
        }

        private static void RunEloquentObjectsBenchmarks(int iterations, int numberOfEventClients)
        {
            var benchmarks = new IBenchmark[]
            {
                new OneWayCalls("pipe", iterations),
                new OneWayCalls("tcp", iterations),
                new EloquentObjects.Proto.Benchmarks.OneWayCalls("pipe", iterations),
                new EloquentObjects.Proto.Benchmarks.OneWayCalls("tcp", iterations),

                new TwoWayCalls("pipe", iterations),
                new TwoWayCalls("tcp", iterations),
                new EloquentObjects.Proto.Benchmarks.TwoWayCalls("pipe", iterations),
                new EloquentObjects.Proto.Benchmarks.TwoWayCalls("tcp", iterations),

                new SumOfTwoIntegers("pipe", iterations),
                new SumOfTwoIntegers("tcp", iterations),

                new Events("pipe", iterations, numberOfEventClients),
                new Events("tcp", iterations, numberOfEventClients),

                new OneWayCallsWithParameter("pipe", iterations),
                new OneWayCallsWithParameter("tcp", iterations), 
                new EloquentObjects.Proto.Benchmarks.OneWayCallsWithParameter("pipe", iterations),
                new EloquentObjects.Proto.Benchmarks.OneWayCallsWithParameter("tcp", iterations)
            };

            foreach (var benchmark in benchmarks)
            {
                var result = benchmark.Measure();
                Console.WriteLine(result);
            }
        }
    }
}