using System.Threading;
using EloquentObjects;

namespace EloquentObjectsBenchmark.EloquentObjects.Benchmarks
{
    internal sealed class Events: IBenchmark
    {
        private readonly string _scheme;
        private readonly int _iterations;
        private readonly int _numberOfEventClients;

        public Events(string scheme, int iterations, int numberOfEventClients)
        {
            _scheme = scheme;
            _iterations = iterations;
            _numberOfEventClients = numberOfEventClients;
        }

        #region Implementation of IBenchmark

        public MeasurementResult Measure()
        {
            using (var remoteObjectServer = new EloquentServer($"{_scheme}://127.0.0.1:50000", new EloquentSettings
            {
                HeartBeatMs = 1000,
                MaxHeartBeatLost = 5,
                ReceiveTimeout = 1000,
                SendTimeout = 1000
            }))
            {
                var benchmarkObject = new BenchmarkObject();
                remoteObjectServer.Add<IBenchmarkObject>("endpoint1", benchmarkObject);

                //Create Clients
                var clients = new EloquentClient[_numberOfEventClients];
                var connections = new IBenchmarkObject[_numberOfEventClients];

                var autoResetEvent = new AutoResetEvent(false);
                for (var i = 0; i < _numberOfEventClients; i++)
                {
                    clients[i] = new EloquentClient($"{_scheme}://127.0.0.1:50000", $"{_scheme}://127.0.0.1:6000{i}", new EloquentSettings
                    {
                        HeartBeatMs = 1000,
                        SendTimeout = 1000,
                        ReceiveTimeout = 10000
                    });
                    connections[i] = clients[i].Get<IBenchmarkObject>("endpoint1");
                    connections[i].EventOccurred += last =>
                    {
                        if (last)
                            autoResetEvent.Set();
                    };
                }

                var result = MeasurementResult.Measure($"EloquentObjects: Events with {_scheme}", () =>
                {
                    benchmarkObject.StartEvents(_iterations / _numberOfEventClients);

                    autoResetEvent.WaitOne();
                });

                //Dispose clients
                for (var i = 0; i < _numberOfEventClients; i++)
                {
                    clients[i].Dispose();
                }

                return result;
            }
        }

        #endregion
    }
}