using System;

namespace EloquentObjectsBenchmark.EloquentObjects
{
    internal sealed class BenchmarkObject : IBenchmarkObject
    {
        #region Implementation of IBenchmarkService

        public void TwoWayCall()
        {
        }

        public void OneWayCall()
        {
        }

        public void OneWayCallWithParameter(Parameter parameter)
        {
        }

        public int Sum(int x, int y)
        {
            return x + y;
        }

        public void StartEvents(int numOfEvents)
        {
            for (var i = 0; i < numOfEvents-1; i++)
                EventOccurred?.Invoke(false);

            //Send true to indicate last event
            EventOccurred?.Invoke(true);
        }

        public event Action<bool> EventOccurred;

        #endregion
    }
}