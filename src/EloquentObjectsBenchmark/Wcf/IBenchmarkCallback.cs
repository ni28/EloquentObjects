using System.ServiceModel;

namespace EloquentObjectsBenchmark.Wcf
{
    [ServiceContract]
    internal interface IBenchmarkCallback
    {
        [OperationContract]
        void EventOccurred();
    }
}