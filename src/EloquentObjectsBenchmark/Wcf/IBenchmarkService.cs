using System.ServiceModel;

namespace EloquentObjectsBenchmark.Wcf
{
    [ServiceContract(CallbackContract = typeof(IBenchmarkCallback))]
    internal interface IBenchmarkService
    {
        [OperationContract(IsOneWay = false)]
        void TwoWayCall();

        [OperationContract(IsOneWay = true)]
        void OneWayCall();

        [OperationContract]
        int Sum(int x, int y);

        [OperationContract(IsOneWay = true)]
        void StartEvents(int numOfEvents);
    }
}