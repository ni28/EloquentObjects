using System;

namespace EloquentObjectsBenchmark
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            const int iterations = 10000;
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
                new Wcf.Benchmarks.OneWayCalls(iterations),
                new Wcf.Benchmarks.TwoWayCalls(iterations),
                new Wcf.Benchmarks.SumOfTwoIntegers(iterations), 
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
                new EloquentObjects.Benchmarks.OneWayCalls(iterations),
                new EloquentObjects.Benchmarks.TwoWayCalls(iterations),
                new EloquentObjects.Benchmarks.SumOfTwoIntegers(iterations),
                new EloquentObjects.Benchmarks.Events(iterations, numberOfEventClients),
                new EloquentObjects.Benchmarks.OneWayCallsWithParameter(iterations), 
                new EloquentObjects.Proto.Benchmarks.OneWayCalls(iterations),
                new EloquentObjects.Proto.Benchmarks.TwoWayCalls(iterations),
                new EloquentObjects.Proto.Benchmarks.OneWayCallsWithParameter(iterations)
            };

            foreach (var benchmark in benchmarks)
            {
                var result = benchmark.Measure();
                Console.WriteLine(result);
            }
        }
    }
}