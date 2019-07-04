using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace EloquentObjectsBenchmark.Wcf.Benchmarks
{
    internal sealed class OneWayCalls: IBenchmark
    {
        private readonly string _scheme;
        private readonly int _iterations;

        public OneWayCalls(string scheme, int iterations)
        {
            _scheme = scheme;
            _iterations = iterations;
        }

        #region Implementation of IBenchmark

        public MeasurementResult Measure()
        {
            var binding = _scheme == "net.tcp"
                ? (Binding)new NetTcpBinding()
                : new NetNamedPipeBinding();
            
            var benchmarkService = new BenchmarkService();
            var serviceHost = new ServiceHost(benchmarkService, new Uri($"{_scheme}://127.0.0.1/Benchmarks"));
            serviceHost.AddServiceEndpoint(typeof(IBenchmarkService), binding, $"{_scheme}://127.0.0.1/Benchmarks");
            serviceHost.Open();

            var channelFactory = new DuplexChannelFactory<IBenchmarkService>(new InstanceContext(new BenchmarkCallback()), binding, new EndpointAddress($"{_scheme}://127.0.0.1/Benchmarks"));
            var channel = channelFactory.CreateChannel();

            var result = MeasurementResult.Measure($"WCF: One-way calls with {_scheme}", () =>
            {
                for (var i = 0; i < _iterations; i++)
                {
                    channel.OneWayCall();
                }
            });

            ((IClientChannel)channel).Close();
            
            serviceHost.Close();
 
            return result;
        }

        #endregion
    }
}