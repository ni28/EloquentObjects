using System;
using System.Collections.Generic;
using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Server.Implementation
{
    internal sealed class EndpointHub : IEndpointHub
    {
        private readonly Dictionary<string, IEndpoint> _endpoints = new Dictionary<string, IEndpoint>();
        private bool _disposed;

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EndpointHub));
            _disposed = true;
            lock (_endpoints)
            {
                foreach (var endpoint in _endpoints.Values) endpoint.Dispose();
            }
        }

        #endregion

        #region Implementation of IEndpointHub

        public bool TryConnectEndpoint(string endpointId, IHostAddress clientHostAddress, int connectionId,
            IOutputChannel outputChannel, out IConnection connection)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EndpointHub));

            connection = null;
            IEndpoint endpoint;
            lock (_endpoints)
            {
                if (!_endpoints.TryGetValue(endpointId, out endpoint))
                    return false;
            }
            
            connection = endpoint.Connect(endpointId, clientHostAddress, connectionId, outputChannel);
            return true;
        }

        public IDisposable AddEndpoint(string endpointId, IEndpoint endpoint)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EndpointHub));

            lock (_endpoints)
            {
                _endpoints.Add(endpointId, endpoint);
            }
            
            return new Disposable(() => RemoveEndpoint(endpointId));
        }

        #endregion

        private void RemoveEndpoint(string endpointId)
        {
            lock (_endpoints)
            {
                _endpoints.Remove(endpointId);
            }
        }
    }
}