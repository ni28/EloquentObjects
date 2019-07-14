using System;
using EloquentObjects.Channels;
using EloquentObjects.Logging;
using EloquentObjects.RPC.Messages.OneWay;
using EloquentObjects.Serialization;

namespace EloquentObjects.RPC.Server.Implementation
{
    internal sealed class Connection : IConnection
    {
        private bool _disposed;
        private readonly int _connectionId;
        private readonly string _objectId;
        private readonly IHostAddress _clientHostAddress;
        private readonly IOutputChannel _outputChannel;
        private readonly ISerializer _serializer;
        private readonly ILogger _logger;


        public Connection(string objectId,
            IHostAddress clientHostAddress, int connectionId,
            IOutputChannel outputChannel, ISerializer serializer)
        {
            _connectionId = connectionId;
            _objectId = objectId;
            _clientHostAddress = clientHostAddress;
            _outputChannel = outputChannel;
            _serializer = serializer;
            
            _logger = Logger.Factory.Create(GetType());
            _logger.Info(() => $"Created (clientHostAddress = {_clientHostAddress}, objectId = {_objectId}, connectionID = {_connectionId})");
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            _disposed = true;
            Disconnected?.Invoke(this, EventArgs.Empty);
            
            _logger.Info(() => $"Disposed (clientHostAddress = {_clientHostAddress}, objectId = {_objectId}, connectionID = {_connectionId})");
        }

        #endregion

        #region Implementation of IConnection

        public void Notify(string eventName, object[] arguments)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Connection));

            var payload = _serializer.SerializeCall(new CallInfo(eventName, arguments));
            var eventMessage = new EventMessage(_clientHostAddress, _objectId, eventName, payload);
            _outputChannel.SendOneWay(eventMessage);
        }

        public void HandleRequest(IInputContext context, RequestMessage requestMessage)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Connection));

            var call = _serializer.DeserializeCall(requestMessage.Payload);
            RequestReceived?.Invoke(context, requestMessage.ClientHostAddress, call);
        }

        public void HandleEvent(EventMessage eventMessage)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Connection));

            var call = _serializer.DeserializeCall(eventMessage.Payload);
            EventReceived?.Invoke(eventMessage.ClientHostAddress, call);
        }

        public event EventHandler Disconnected;
        public event Action<IHostAddress, CallInfo> EventReceived;
        public event Action<IInputContext, IHostAddress, CallInfo> RequestReceived;

        #endregion
    }
}