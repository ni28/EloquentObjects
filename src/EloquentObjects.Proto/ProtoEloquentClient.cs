namespace EloquentObjects.Proto
{
    public sealed class ProtoEloquentClient : IEloquentClient
    {
        private readonly EloquentClient _eloquentClient;

        public ProtoEloquentClient(string serverIpPort, string clientIpPort)
        {
            _eloquentClient = new EloquentClient(serverIpPort, clientIpPort);
        }
        
        public ProtoEloquentClient(string serverIpPort, string clientIpPort, EloquentSettings settings)
        {
            _eloquentClient = new EloquentClient(serverIpPort, clientIpPort, settings);
        }
        
        #region Implementation of IEloquentClient

        public Connection<T> Connect<T>(string objectId) where T : class
        {
            return _eloquentClient.Connect<T>(objectId);
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