using System;
using System.Diagnostics;

namespace EloquentObjectsBenchmark
{
    internal sealed class MeasurementResult
    {
        public MeasurementResult(string measurementName, long elapsedMilliseconds)
        {
            MeasurementName = measurementName;
            ElapsedMilliseconds = elapsedMilliseconds;
        }

        public static MeasurementResult Measure(string measurementName, Action action)
        {
            var sw = new Stopwatch();
            sw.Start();
            action();
            sw.Stop();

            return new MeasurementResult(measurementName, sw.ElapsedMilliseconds);
        }

        public string MeasurementName { get; }
        public long ElapsedMilliseconds { get; }

        #region Overrides of Object

        public override string ToString()
        {
            return $"{MeasurementName}: {ElapsedMilliseconds} ms.";
        }

        #endregion
    }
}