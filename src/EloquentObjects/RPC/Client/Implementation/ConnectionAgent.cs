using System;
using System.IO;
using EloquentObjects.Channels;
using EloquentObjects.Contracts;
using EloquentObjects.Logging;
using EloquentObjects.RPC.Messages;
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
            var disconnectMessage = new DisconnectMessage(_clientHostAddress, ConnectionId);

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
            var payload = _serializer.SerializeCall(new CallInfo(eventName, arguments));
            var eventMessage = new EventMessage(_clientHostAddress, _endpointId, ConnectionId, payload);

            using (var context = _outputChannel.BeginWriteRead())
            {
                eventMessage.Write(context.Stream);
            }
        }

        public object Call(IEloquentClient eloquentClient, IContractDescription contractDescription, string methodName,
            object[] parameters)
        {
            var payload = _serializer.SerializeCall(new CallInfo(methodName, parameters));
            var requestMessage = new RequestMessage(_clientHostAddress, _endpointId, ConnectionId, payload);

            using (var context = _outputChannel.BeginWriteRead())
            {
                requestMessage.Write(context.Stream);

                var result = Message.Read(context.Stream);
                switch (result)
                {
                    case ExceptionMessage exceptionSessionMessage:
                        throw exceptionSessionMessage.Exception;
                    case EloquentObjectMessage eloquentObjectMessage:
                        var clientType = contractDescription.GetOperationDescription(methodName, parameters).Method.ReturnType.GenericTypeArguments[0];
                        var eloquentType = typeof(ClientEloquentObject<>).MakeGenericType(clientType);
                        return Activator.CreateInstance(eloquentType, eloquentObjectMessage.ObjectId, null, eloquentClient);
                    case ResponseMessage responseMessage:
                        return _serializer.Deserialize<object>(responseMessage.Payload);
                    default:
                        throw new IOException($"Unexpected session message type: {result.MessageType}");
                }
            }
        }

        public event EventHandler<NotifyEventArgs> EventReceived;
        
        public void HandleEvent(EventMessage eventMessage)
        {
            var callInfo = _serializer.DeserializeCall(eventMessage.Payload);
            EventReceived?.Invoke(this, new NotifyEventArgs(callInfo.OperationName, callInfo.Parameters));
        }

        #endregion

    }
}