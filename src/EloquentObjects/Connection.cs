using System;
using EloquentObjects.RPC.Client.Implementation;

namespace EloquentObjects
{
    public sealed class Connection<T> : IDisposable
    {
        private readonly ClientInterceptor _interceptor;
        private readonly T _proxy;
        private bool _disposed;

        internal Connection(ClientInterceptor interceptor, T proxy)
        {
            _interceptor = interceptor;
            _proxy = proxy;
        }

        public T Object
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(Connection<T>));
                return _proxy;
            }
        }

        #region IDisposable

        public void Dispose()
        {
            _disposed = true;
            _interceptor.Dispose();
        }

        #endregion
    }
}