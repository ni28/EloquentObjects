using EloquentObjects;

namespace EloquentObjectsBenchmark.EloquentObjects.Benchmarks
{
    internal sealed class TwoWayCalls: IBenchmark
    {
        private readonly string _scheme;
        private readonly int _iterations;

        public TwoWayCalls(string scheme, int iterations)
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

                var benchmarkObj = remoteObjectClient.Connect<IBenchmarkObject>("endpoint1");

                return MeasurementResult.Measure($"EloquentObjects: Two-way calls with {_scheme}", () =>
                {
                    for (var i = 0; i < _iterations; i++)
                    {
                        benchmarkObj.TwoWayCall();
                    }
                });
            }
        }

        #endregion
    }
}