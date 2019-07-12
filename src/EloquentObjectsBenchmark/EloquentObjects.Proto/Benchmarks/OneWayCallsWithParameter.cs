using EloquentObjects;

namespace EloquentObjectsBenchmark.EloquentObjects.Proto.Benchmarks
{
    internal sealed class OneWayCallsWithParameter: IBenchmark
    {
        private readonly string _scheme;
        private readonly int _iterations;

        public OneWayCallsWithParameter(string scheme, int iterations)
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

                return MeasurementResult.Measure($"EloquentObjects.Proto: One-way calls with parameter with {_scheme}",
                    () =>
                    {
                        for (var i = 0; i < _iterations; i++)
                        {
                            benchmarkObj.OneWayCallWithParameter(parameter);
                        }
                    });

            }
        }

        #endregion
    }
}