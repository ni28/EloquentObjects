namespace EloquentObjects.RPC.Client.Implementation
{
    internal sealed class ClientEloquentObject<T>: IEloquent<T> where T : class
    {
        private readonly IEloquentClient _client;

        public ClientEloquentObject(string objectId, object info, IEloquentClient client)
        {
            _client = client;
            ObjectId = objectId;
            Info = info;
        }

        #region Implementation of IDisposable

        public IConnection<T> Connect()
        {
            return _client.Connect<T>(ObjectId);
        }

        #endregion

        #region Implementation of IEloquent

        public string ObjectId { get; }
        public object Info { get; }

        #endregion
    }
}