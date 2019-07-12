using EloquentObjects;

namespace EloquentObjectsBenchmark.EloquentObjects.Benchmarks
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
            using (var remoteObjectServer = new EloquentServer($"{_scheme}://127.0.0.1:50000"))
            using (var remoteObjectClient = new EloquentClient($"{_scheme}://127.0.0.1:50000", $"{_scheme}://127.0.0.1:50001"))
            {
                remoteObjectServer.Add<IBenchmarkObject>("endpoint1", new BenchmarkObject());

                var benchmarkObj = remoteObjectClient.Get<IBenchmarkObject>("endpoint1");

                return MeasurementResult.Measure($"EloquentObjects: One-way calls with {_scheme}", () =>
                {
                    for (var i = 0; i < _iterations; i++)
                    {
                        benchmarkObj.OneWayCall();
                    }
                });
            }
        }

        #endregion
    }
}