using System;
using System.IO;
using System.Linq;
using EloquentObjects.Channels;
using EloquentObjects.Contracts;
using EloquentObjects.RPC.Messages;
using EloquentObjects.RPC.Messages.OneWay;
using EloquentObjects.Serialization;

namespace EloquentObjects.RPC.Client.Implementation
{
    internal sealed class Connection: IConnection
    {
        private readonly string _objectId;
        private readonly object _outerProxy;
        private readonly IEventHandlersRepository _eventHandlersRepository;
        private readonly IContractDescription _contractDescription;
        private readonly ISerializer _serializer;
        private readonly IEloquentClient _eloquentClient;
        private readonly IOutputChannel _outputChannel;
        private readonly IHostAddress _clientHostAddress;

        public Connection(string objectId,
            object outerProxy,
            IEventHandlersRepository eventHandlersRepository,
            IContractDescription contractDescription,
            ISerializer serializer,
            IEloquentClient eloquentClient,
            IOutputChannel outputChannel,
            IHostAddress clientHostAddress)
        {
            _objectId = objectId;
            _outerProxy = outerProxy;
            _eventHandlersRepository = eventHandlersRepository;
            _contractDescription = contractDescription;
            _serializer = serializer;
            _eloquentClient = eloquentClient;
            _outputChannel = outputChannel;
            _clientHostAddress = clientHostAddress;
        }

        #region Implementation of IConnection

        public void Notify(string eventName, object[] parameters)
        {
            var payload = _serializer.SerializeCall(new CallInfo(eventName, parameters));
            var eventMessage = new NotificationMessage(_clientHostAddress, _objectId, payload);
            _outputChannel.Send(eventMessage);
        }

        public object Call(string methodName, object[] parameters)
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
                    var objectType = _contractDescription.GetOperationDescription(methodName, parameters).Method.ReturnType;
                    return _eloquentClient.Connect(objectType, eloquentObjectMessage.ObjectId);
                case ResponseMessage responseMessage:
                    return _serializer.Deserialize<object>(responseMessage.Payload);
                default:
                    throw new IOException($"Unexpected session message type: {result.MessageType}");
            }
        }

        public void Subscribe(string eventName, Delegate handler)
        {
            var eventDescription = _contractDescription.Events.First(e => e.Name == eventName);

            _eventHandlersRepository.Subscribe(_objectId, eventDescription, _serializer, handler, _outerProxy);
        }

        public void Unsubscribe(string eventName, Delegate handler)
        {
            _eventHandlersRepository.Unsubscribe(_objectId, eventName, handler);
        }

        #endregion
        
    }
}