using System.ServiceModel;

namespace EloquentObjectsBenchmark.Wcf
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true, UseSynchronizationContext = true)]
    internal sealed class BenchmarkService : IBenchmarkService
    {
        #region Implementation of IBenchmarkService

        public void TwoWayCall()
        {
        }

        public void OneWayCall()
        {
        }

        public int Sum(int x, int y)
        {
            return x + y;
        }

        public void StartEvents(int numOfEvents)
        {
            /*
            for (var i = 0; i < numOfEvents-1; i++)
                EventOccurred?.Invoke(false);

            //Send true to indicate last event
            EventOccurred?.Invoke(true);
            */
        }

        #endregion
    }
}