using System;
using System.Collections.Generic;
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
            SplitParameters(parameters, out var serializedParameters, out var objectIds, out var selector);

            var payload = _serializer.Serialize(serializedParameters);
            var eventMessage = new NotificationMessage(_clientHostAddress, _objectId, methodName, payload, selector, objectIds);
            _outputChannel.SendOneWay(eventMessage);
        }

        public object Call(string methodName, object[] parameters)
        {
            SplitParameters(parameters, out var serializedParameters, out var objectIds, out var selector);

            var payload = _serializer.Serialize(serializedParameters);

            var requestMessage = new RequestMessage(_clientHostAddress, _objectId, methodName, payload, selector, objectIds);

            var result = _outputChannel.SendAndReceive(requestMessage);
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
                    return _serializer.Deserialize(responseMessage.Payload).Single();
                default:
                    throw new IOException($"Unexpected session message type: {result.MessageType}");
            }
        }

        private void SplitParameters(IReadOnlyCollection<object> parameters, out object[] serializedParameters, out string[] objectIds, out bool[] selector)
        {
            var selectorList = new List<bool>(parameters.Count);
            var serializedParametersList = new List<object>(parameters.Count);
            var objectIdsList = new List<string>(parameters.Count);

            //Split parameters to two groups - remote objects and serializable objects.
            //Use selector to store a flag that indicates if a parameter is a remote object.
            foreach (var parameter in parameters)
            {
                if (_eloquentClient.TryGetObjectId(parameter, out var objectId))
                {
                    objectIdsList.Add(objectId);
                    selectorList.Add(true);
                }
                else
                {
                    serializedParametersList.Add(parameter);
                    selectorList.Add(false);
                }
            }

            serializedParameters = serializedParametersList.ToArray();
            objectIds = objectIdsList.ToArray();
            selector = selectorList.ToArray();
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