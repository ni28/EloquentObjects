using System;
using System.ServiceModel;

namespace EloquentObjectsBenchmark.Wcf.Benchmarks
{
    internal sealed class SumOfTwoIntegers: IBenchmark
    {
        private readonly int _iterations;

        public SumOfTwoIntegers(int iterations)
        {
            _iterations = iterations;
        }

        #region Implementation of IBenchmark

        public MeasurementResult Measure()
        {
            var binding = new NetTcpBinding();
            
            var benchmarkService = new BenchmarkService();
            var serviceHost = new ServiceHost(benchmarkService, new Uri("net.tcp://127.0.0.1/Benchmarks"));
            serviceHost.AddServiceEndpoint(typeof(IBenchmarkService), binding, "net.tcp://127.0.0.1/Benchmarks");
            serviceHost.Open();

            var channelFactory = new DuplexChannelFactory<IBenchmarkService>(new InstanceContext(new BenchmarkCallback()), binding, new EndpointAddress("net.tcp://127.0.0.1/Benchmarks"));
            var channel = channelFactory.CreateChannel();

            var result = MeasurementResult.Measure("WCF: Sum of two integers", () =>
            {
                for (var i = 0; i < _iterations; i++)
                {
                    channel.Sum(i, i);
                }
            });

            ((IClientChannel)channel).Close();
            
            serviceHost.Close();
 
            return result;
        }

        #endregion
    }
}