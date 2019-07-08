using System.Threading;

namespace EloquentObjects.Proto
{
    public sealed class ProtoEloquentServer: IEloquentServer
    {
        private readonly EloquentServer _eloquentServer;

        public ProtoEloquentServer(string address)
        {
            _eloquentServer = new EloquentServer(address);
        }
        
        public ProtoEloquentServer(string address, EloquentSettings settings)
        {
            _eloquentServer = new EloquentServer(address, settings);
        }
        
        #region Implementation of IEloquentServer

        public IObjectHost<T> Add<T>(string objectId, T obj, SynchronizationContext synchronizationContext = null) where T: class
        {
            return _eloquentServer.Add(objectId, obj, synchronizationContext);
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