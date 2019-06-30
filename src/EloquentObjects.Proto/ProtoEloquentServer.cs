using System.Threading;

namespace EloquentObjects.Proto
{
    public sealed class ProtoEloquentServer: IEloquentServer
    {
        private readonly EloquentServer _eloquentServer;

        public ProtoEloquentServer(string serverIpPort)
        {
            _eloquentServer = new EloquentServer(serverIpPort);
        }
        
        public ProtoEloquentServer(string serverIpPort, EloquentSettings settings)
        {
            _eloquentServer = new EloquentServer(serverIpPort, settings);
        }
        
        #region Implementation of IEloquentServer

        public void Add<T>(string objectId, T obj, SynchronizationContext synchronizationContext = null)
        {
            _eloquentServer.Add(objectId, obj, synchronizationContext);
        }

        #endregion
            
        #region Implementation of IDisposable

        public void Dispose()
        {
            _eloquentServer.Dispose();
        }

        #endregion
    }
}