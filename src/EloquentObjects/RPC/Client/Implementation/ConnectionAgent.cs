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
        private readonly string _objectId;
        private readonly IOutputChannel _outputChannel;
        private readonly ILogger _logger;

        public ConnectionAgent(int connectionId, string objectId,
            IOutputChannel outputChannel, IHostAddress clientHostAddress, ISerializer serializer)
        {
            ConnectionId = connectionId;
            _objectId = objectId;
            _outputChannel = outputChannel;
            _clientHostAddress = clientHostAddress;
            _serializer = serializer;

            _logger = Logger.Factory.Create(GetType());
            _logger.Info(() => $"Created (connectionId = {ConnectionId}, objectId = {_objectId}, clientHostAddress = {_clientHostAddress})");
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            var disconnectMessage = new DisconnectMessage(_clientHostAddress, ConnectionId);
            _outputChannel.Write(disconnectMessage.ToFrame());

            _logger.Info(() => $"Disposed (connectionId = {ConnectionId}, objectId = {_objectId}, clientHostAddress = {_clientHostAddress})");
        }

        #endregion

        #region Implementation of IConnectionAgent

        public int ConnectionId { get; }

        public void Notify(string eventName, object[] arguments)
        {
            var payload = _serializer.SerializeCall(new CallInfo(eventName, arguments));
            var eventMessage = new EventMessage(_clientHostAddress, _objectId, ConnectionId, payload);
            _outputChannel.Write(eventMessage.ToFrame());
        }

        public object Call(IEloquentClient eloquentClient, IContractDescription contractDescription, string methodName,
            object[] parameters)
        {
            var payload = _serializer.SerializeCall(new CallInfo(methodName, parameters));
            var requestMessage = new RequestMessage(_clientHostAddress, _objectId, ConnectionId, payload);

            _outputChannel.Write(requestMessage.ToFrame());
            
            var result = Message.Create(_outputChannel.Read());
            switch (result)
            {
                case ExceptionMessage exceptionMessage:
                    throw exceptionMessage.Exception;
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

        public event EventHandler<NotifyEventArgs> EventReceived;
        
        public void HandleEvent(EventMessage eventMessage)
        {
            var callInfo = _serializer.DeserializeCall(eventMessage.Payload);
            EventReceived?.Invoke(this, new NotifyEventArgs(callInfo.OperationName, callInfo.Parameters));
        }

        #endregion

    }
}