using System;

namespace EloquentObjects.Proto
{
    public sealed class ProtoEloquentClient : IEloquentClient
    {
        private readonly EloquentClient _eloquentClient;

        public ProtoEloquentClient(string serverAddress, string clientAddress)
        {
            _eloquentClient = new EloquentClient(serverAddress, clientAddress);
        }
        
        public ProtoEloquentClient(string serverAddress, string clientAddress, EloquentSettings settings)
        {
            _eloquentClient = new EloquentClient(serverAddress, clientAddress, settings);
        }
        
        #region Implementation of IEloquentClient

        public T Connect<T>(string objectId) where T : class
        {
            return _eloquentClient.Connect<T>(objectId);
        }

        public object Connect(Type type, string objectId)
        {
            return _eloquentClient.Connect(type, objectId);
        }

        public bool TryGetObjectId(object remoteObject, out string objectId)
        {
            return _eloquentClient.TryGetObjectId(remoteObject, out objectId);
        }

        #endregion
        
        #region Implementation of IDisposable

        public void Dispose()
        {
            _eloquentClient.Dispose();
        }

        #endregion
    }
}