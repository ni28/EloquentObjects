using System;
using System.IO;
using System.Linq;
using EloquentObjects.Channels;
using EloquentObjects.Contracts;
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

        public void Notify(string methodName, object[] parameters)
        {
            var payload = Payload.Create(parameters, o => _eloquentClient.TryGetObjectId(o, out var id) ? id : null);
            var notificationMessage = payload.CreateNotificationMessage(_serializer, _clientHostAddress, _objectId, methodName);
            _outputChannel.SendOneWay(notificationMessage);
        }

        public object Call(string methodName, object[] parameters)
        {
            var payload = Payload.Create(parameters, o => _eloquentClient.TryGetObjectId(o, out var id) ? id : null);
            var requestMessage = payload.CreateRequestMessage(_serializer, _clientHostAddress, _objectId, methodName);

            var result = _outputChannel.SendAndReceive(requestMessage);
            switch (result)
            {
                case ErrorMessage errorMessage:
                    throw errorMessage.ToException();
                case ExceptionMessage exceptionMessage:
                    throw exceptionMessage.Exception;
                case ResponseMessage responseMessage:
                    if (responseMessage.Selector[0])
                    {
                        var objectType = _contractDescription.GetOperationDescription(methodName, parameters).Method.ReturnType;
                        return _eloquentClient.Connect(objectType, responseMessage.ObjectIds[0]);
                    }
                    else
                    {
                        return _serializer.Deserialize(responseMessage.SerializedParameters).Single();
                    }

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