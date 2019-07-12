using System;
using System.IO;
using EloquentObjects.Channels;
using EloquentObjects.Contracts;
using EloquentObjects.Logging;
using EloquentObjects.RPC.Messages;
using EloquentObjects.RPC.Messages.Acknowledged;
using EloquentObjects.RPC.Messages.OneWay;
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

        public ConnectionAgent(string objectId,
            IOutputChannel outputChannel, IHostAddress clientHostAddress, ISerializer serializer)
        {
            _objectId = objectId;
            _outputChannel = outputChannel;
            _clientHostAddress = clientHostAddress;
            _serializer = serializer;

            _logger = Logger.Factory.Create(GetType());
            _logger.Info(() => $"Created (objectId = {_objectId}, clientHostAddress = {_clientHostAddress})");
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            _logger.Info(() => $"Disposed (objectId = {_objectId}, clientHostAddress = {_clientHostAddress})");
        }

        #endregion

        #region Implementation of IConnectionAgent

        public void Notify(string eventName, object[] arguments)
        {
            var payload = _serializer.SerializeCall(new CallInfo(eventName, arguments));
            var eventMessage = new NotificationMessage(_clientHostAddress, _objectId, payload);
            _outputChannel.Send(eventMessage);
        }

        public object Call(IEloquentClient eloquentClient, IContractDescription contractDescription, string methodName,
            object[] parameters)
        {
            var payload = _serializer.SerializeCall(new CallInfo(methodName, parameters));
            var requestMessage = new RequestMessage(_clientHostAddress, _objectId, payload);
            _outputChannel.Send(requestMessage);
            
            //TODO: move to RequestMessage?
            var result = Message.Create(_outputChannel.Read());
            switch (result)
            {
                case ErrorMessage errorMessage:
                    throw errorMessage.ToException();
                case ExceptionMessage exceptionMessage:
                    throw exceptionMessage.Exception;
                case EloquentObjectMessage eloquentObjectMessage:
                    var objectType = contractDescription.GetOperationDescription(methodName, parameters).Method.ReturnType;
                    return eloquentClient.Get(objectType, eloquentObjectMessage.ObjectId);
                case ResponseMessage responseMessage:
                    return _serializer.Deserialize<object>(responseMessage.Payload);
                default:
                    throw new IOException($"Unexpected session message type: {result.MessageType}");
            }
        }

        public void Subscribe(string eventName)
        {
            var subscribeMessage = new SubscribeEventMessage(_clientHostAddress, _objectId, eventName);
            _outputChannel.SendWithAck(subscribeMessage);
        }

        public void Unsubscribe(string eventName)
        {
            var unsubscribeMessage = new UnsubscribeEventMessage(_clientHostAddress, _objectId, eventName);
            _outputChannel.SendWithAck(unsubscribeMessage);
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