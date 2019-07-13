using System;

namespace EloquentObjectsBenchmark.EloquentObjects
{
    public interface IBenchmarkObject
    {
        void TwoWayCall();

        void OneWayCall();

        void OneWayCallWithParameter(Parameter parameter);

        int Sum(int x, int y);

        void StartEvents(int numOfEvents);
        
        event Action<bool> EventOccurred;
    }
}