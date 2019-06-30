using System;
using System.IO;
using EloquentObjects.Channels;
using EloquentObjects.Logging;
using EloquentObjects.RPC.Messages.Endpoint;
using EloquentObjects.RPC.Messages.Session;
using EloquentObjects.Serialization;

namespace EloquentObjects.RPC.Server.Implementation
{
    internal sealed class Connection : IConnection
    {
        private bool _disposed;
        private readonly int _connectionId;
        private readonly string _endpointId;
        private readonly IHostAddress _clientHostAddress;
        private readonly IOutputChannel _outputChannel;
        private readonly ISerializer _serializer;
        private readonly ILogger _logger;


        public Connection(string endpointId,
            IHostAddress clientHostAddress, int connectionId,
            IOutputChannel outputChannel, ISerializer serializer)
        {
            _connectionId = connectionId;
            _endpointId = endpointId;
            _clientHostAddress = clientHostAddress;
            _outputChannel = outputChannel;
            _serializer = serializer;
            
            _logger = Logger.Factory.Create(GetType());
            _logger.Info(() => $"Created (clientHostAddress = {_clientHostAddress}, endpointId = {_endpointId}, connectionID = {_connectionId})");
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            _disposed = true;
            Disconnected?.Invoke(this, EventArgs.Empty);
            
            _logger.Info(() => $"Disposed (clientHostAddress = {_clientHostAddress}, endpointId = {_endpointId}, connectionID = {_connectionId})");
        }

        #endregion

        #region Implementation of IConnection

        public void Notify(string eventName, object[] arguments)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Connection));

            var endpointMessageStart = new EndpointRequestStartSessionMessage(_clientHostAddress, _connectionId);
            var eventMessage = new EventEndpointMessage(_endpointId, _connectionId, eventName, arguments);

            using (var context = _outputChannel.BeginWriteRead())
            {
                endpointMessageStart.Write(context.Stream);
                eventMessage.Write(context.Stream, _serializer);
            }
        }

        public void Receive(Stream stream, IHostAddress clientHostAddress)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Connection));

            MessageReady?.Invoke(stream, clientHostAddress);
        }

        public event EventHandler Disconnected;
        public event Action<Stream, IHostAddress> MessageReady;

        #endregion
    }
}