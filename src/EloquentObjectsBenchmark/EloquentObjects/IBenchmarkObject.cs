using System;
using EloquentObjects;

namespace EloquentObjectsBenchmark.EloquentObjects
{
    [EloquentInterface]
    public interface IBenchmarkObject
    {
        [EloquentMethod(IsOneWay = false)]
        void TwoWayCall();

        [EloquentMethod(IsOneWay = true)]
        void OneWayCall();

        [EloquentMethod(IsOneWay = true)]
        void OneWayCallWithParameter(Parameter parameter);

        [EloquentMethod]
        int Sum(int x, int y);

        [EloquentMethod(IsOneWay = true)]
        void StartEvents(int numOfEvents);
        
        [EloquentEvent]
        event Action<bool> EventOccurred;
    }
}