using EloquentObjects;
using EloquentObjects.Proto;

namespace EloquentObjectsBenchmark.EloquentObjects.Proto.Benchmarks
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
            using (var remoteObjectServer = new ProtoEloquentServer("127.0.0.1:50000"))
            using (var remoteObjectClient = new ProtoEloquentClient("127.0.0.1:50000", "127.0.0.1:50001"))
            {
                remoteObjectServer.Add<EloquentObjects.IBenchmarkObject>("endpoint1", new EloquentObjects.BenchmarkObject());

                using (var session = remoteObjectClient.Connect<EloquentObjects.IBenchmarkObject>("endpoint1"))
                {
                    var benchmarkObj = session.Object;

                    return MeasurementResult.Measure("Proto One-way calls", () =>
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