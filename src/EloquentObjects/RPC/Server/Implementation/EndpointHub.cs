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

        private IEndpoint GetEndpoint(string endpointId)
        {
            IEndpoint endpoint;
            lock (_endpoints)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(EndpointHub));
                if (!_endpoints.TryGetValue(endpointId, out endpoint))
                    throw new InvalidOperationException($"Endpoint with address {endpointId} was not found");
            }

            return endpoint;
        }

        #region Implementation of IEndpointHub

        public IConnection ConnectEndpoint(string endpointId, IHostAddress clientHostAddress, int connectionId,
            IOutputChannel outputChannel)
        {
            var endpoint = GetEndpoint(endpointId);
            return endpoint.Connect(endpointId, clientHostAddress, connectionId, outputChannel);
        }

        public IDisposable AddEndpoint(string endpointId, IEndpoint endpoint)
        {
            lock (_endpoints)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(EndpointHub));

                _endpoints.Add(endpointId, endpoint);
            }
            
            return new Disposable(() => RemoveEndpoint(endpointId));
        }

        public bool ContainsEndpoint(string endpointId)
        {
            lock (_endpoints)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(EndpointHub));

                return _endpoints.ContainsKey(endpointId);
            }
        }

        #endregion

        private void RemoveEndpoint(string endpointId)
        {
            lock (_endpoints)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(EndpointHub));

                _endpoints.Remove(endpointId);
            }
        }
    }
}