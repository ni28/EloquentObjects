using System;
using System.IO;
using EloquentObjects.Channels;
using EloquentObjects.Logging;
using EloquentObjects.RPC.Messages;
using EloquentObjects.RPC.Messages.Endpoint;
using EloquentObjects.RPC.Messages.Session;
using EloquentObjects.Serialization;

namespace EloquentObjects.RPC.Client.Implementation
{
    //TODO: Better name is needed to avoid confusion with Connection
    internal sealed class ConnectionAgent : IConnectionAgent
    {
        private readonly IHostAddress _clientHostAddress;
        private readonly ISerializer _serializer;
        private readonly string _endpointId;
        private readonly IOutputChannel _outputChannel;
        private readonly ILogger _logger;

        public ConnectionAgent(int connectionId, string endpointId,
            IOutputChannel outputChannel, IHostAddress clientHostAddress, ISerializer serializer)
        {
            ConnectionId = connectionId;
            _endpointId = endpointId;
            _outputChannel = outputChannel;
            _clientHostAddress = clientHostAddress;
            _serializer = serializer;

            _logger = Logger.Factory.Create(GetType());
            _logger.Info(() => $"Created (connectionId = {ConnectionId}, endpointId = {_endpointId}, clientHostAddress = {_clientHostAddress})");
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            var disconnectMessage = new DisconnectSessionMessage(_clientHostAddress, ConnectionId);

            using (var context = _outputChannel.BeginWriteRead())
            {
                disconnectMessage.Write(context.Stream);
            }

            _logger.Info(() => $"Disposed (connectionId = {ConnectionId}, endpointId = {_endpointId}, clientHostAddress = {_clientHostAddress})");
        }

        #endregion

        #region Implementation of IConnectionAgent

        public int ConnectionId { get; }

        public void Notify(string eventName, object[] arguments)
        {
            var endpointMessageStart = new EndpointRequestStartSessionMessage(_clientHostAddress, ConnectionId);
            var eventMessage = new EventEndpointMessage(_endpointId, ConnectionId, eventName, arguments);

            using (var context = _outputChannel.BeginWriteRead())
            {
                endpointMessageStart.Write(context.Stream);
                eventMessage.Write(context.Stream, _serializer);
            }
        }

        public object Call(string methodName, object[] parameters)
        {
            var endpointMessageStart = new EndpointRequestStartSessionMessage(_clientHostAddress, ConnectionId);
            var requestMessage = new RequestEndpointMessage(_endpointId, ConnectionId, methodName, parameters);

            using (var context = _outputChannel.BeginWriteRead())
            {
                endpointMessageStart.Write(context.Stream);
                requestMessage.Write(context.Stream, _serializer);

                var responseSessionMessage = SessionMessage.Read(context.Stream);
                switch (responseSessionMessage)
                {
                    case ExceptionSessionMessage exceptionSessionMessage:
                        throw exceptionSessionMessage.Exception;
                    case EndpointResponseStartSessionMessage _:
                        var response = (ResponseEndpointMessage)EndpointMessage.Read(context.Stream, _serializer);
                        return response.Response;
                    default:
                        throw new IOException($"Unexpected session message type: {responseSessionMessage.MessageType}");
                }
            }
        }

        public event EventHandler<NotifyEventArgs> EventReceived;

        public void ReceiveAndHandleEndpointMessage(Stream stream)
        {
            var endpointMessage = EndpointMessage.Read(stream, _serializer);
            switch (endpointMessage)
            {
                case EventEndpointMessage eventMessage:
                    EventReceived?.Invoke(this, new NotifyEventArgs(eventMessage.EventName, eventMessage.Arguments));
                    break;
                default:
                    //No profit to raise exception as this is running in input channel thread
                    return;
            }
        }

        #endregion

    }
}