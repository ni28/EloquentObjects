using EloquentObjects.Proto;

namespace EloquentObjectsBenchmark.EloquentObjects.Proto.Benchmarks
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
            using (var remoteObjectServer = new ProtoEloquentServer($"{_scheme}://127.0.0.1:50000"))
            using (var remoteObjectClient = new ProtoEloquentClient($"{_scheme}://127.0.0.1:50000", $"{_scheme}://127.0.0.1:50001"))
            {
                remoteObjectServer.Add<EloquentObjects.IBenchmarkObject>("endpoint1", new EloquentObjects.BenchmarkObject());

                using (var session = remoteObjectClient.Connect<EloquentObjects.IBenchmarkObject>("endpoint1"))
                {
                    var benchmarkObj = session.Object;

                    return MeasurementResult.Measure($"EloquentObjects.Proto: One-way calls with {_scheme}", () =>
                    {
                        for (var i = 0; i < _iterations; i++)
                        {
                            benchmarkObj.OneWayCall();
                        }
                    });
                }
            }
        }

        #endregion
    }
}