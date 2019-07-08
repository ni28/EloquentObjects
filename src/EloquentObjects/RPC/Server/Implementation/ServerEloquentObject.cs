using System;

namespace EloquentObjects.RPC.Server.Implementation
{
    internal sealed class ServerEloquentObject<T>: IObjectHost<T> where T : class
    {
        private readonly IDisposable _objectHost;

        public ServerEloquentObject(string objectId, object info, IDisposable objectHost)
        {
            _objectHost = objectHost;
            ObjectId = objectId;
            Info = info;
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            _objectHost.Dispose();
        }

        #endregion

        #region Implementation of IEloquent<out T>

        public string ObjectId { get; }
        
        public object Info { get; }
        
        public IConnection<T> Connect()
        {
            throw new InvalidOperationException("The Connect method is intended to be called on client only");
        }

        #endregion
    }
}