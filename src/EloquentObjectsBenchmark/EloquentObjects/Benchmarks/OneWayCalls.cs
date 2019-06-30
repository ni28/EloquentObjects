using EloquentObjects;

namespace EloquentObjectsBenchmark.EloquentObjects.Benchmarks
{
    internal sealed class OneWayCalls: IBenchmark
    {
        private readonly int _iterations;

        public OneWayCalls(int iterations)
        {
            _iterations = iterations;
        }

        #region Implementation of IBenchmark

        public MeasurementResult Measure()
        {
            using (var remoteObjectServer = new EloquentServer("127.0.0.1:50000"))
            using (var remoteObjectClient = new EloquentClient("127.0.0.1:50000", "127.0.0.1:50001"))
            {
                remoteObjectServer.Add<IBenchmarkObject>("endpoint1", new BenchmarkObject());

                using (var session = remoteObjectClient.Connect<IBenchmarkObject>("endpoint1"))
                {
                    var benchmarkObj = session.Object;

                    return MeasurementResult.Measure("One-way calls", () =>
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