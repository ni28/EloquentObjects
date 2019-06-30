using EloquentObjects;

namespace EloquentObjectsBenchmark.EloquentObjects.Benchmarks
{
    internal sealed class OneWayCallsWithParameter: IBenchmark
    {
        private readonly int _iterations;

        public OneWayCallsWithParameter(int iterations)
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

                    var parameter = new Parameter
                    {
                        Parameter1 = new InnerParameter1
                        {
                            Parameter2 = new InnerParameter2
                            {
                                Parameter3 = new InnerParameter3
                                {
                                    IntValue = 123,
                                    BoolValue = false,
                                    DoubleValue = 123.123,
                                    StringValue = "123"
                                }
                            }
                        }
                    };
                    
                    return MeasurementResult.Measure("One-way calls with parameter", () =>
                    {
                        for (var i = 0; i < _iterations; i++)
                        {
                            benchmarkObj.OneWayCallWithParameter(parameter);
                        }
                    });
                }
            }
        }

        #endregion
    }
}