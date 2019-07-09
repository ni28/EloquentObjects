using System;
using System.IO;
using EloquentObjects.Channels;
using EloquentObjects.Logging;
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

            var payload = _serializer.SerializeCall(new CallInfo(eventName, arguments));
            var eventMessage = new EventMessage(_clientHostAddress, _endpointId, _connectionId, payload);

            using (var context = _outputChannel.BeginWriteRead())
            {
                eventMessage.Write(context.Stream);
            }
        }

        public void HandleRequest(Stream stream, RequestMessage requestMessage)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Connection));

            var call = _serializer.DeserializeCall(requestMessage.Payload);
            RequestReceived?.Invoke(stream, requestMessage.ClientHostAddress, call);
        }

        public void HandleEvent(Stream stream, EventMessage eventMessage)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Connection));

            var call = _serializer.DeserializeCall(eventMessage.Payload);
            EventReceived?.Invoke(eventMessage.ClientHostAddress, call);
        }

        public event EventHandler Disconnected;
        public event Action<IHostAddress, CallInfo> EventReceived;
        public event Action<Stream, IHostAddress, CallInfo> RequestReceived;

        #endregion
    }
}