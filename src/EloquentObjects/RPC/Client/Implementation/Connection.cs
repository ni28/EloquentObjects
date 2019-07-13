using System;
using System.Linq;
using EloquentObjects.Contracts;
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
        private readonly EloquentClient _eloquentClient;
        private readonly IConnectionAgent _connectionAgent;

        public Connection(string objectId,
            object outerProxy,
            ISessionAgent sessionAgent,
            IEventHandlersRepository eventHandlersRepository,
            IContractDescription contractDescription,
            ISerializer serializer,
            EloquentClient eloquentClient)
        {
            _objectId = objectId;
            _outerProxy = outerProxy;
            _eventHandlersRepository = eventHandlersRepository;
            _contractDescription = contractDescription;
            _serializer = serializer;
            _eloquentClient = eloquentClient;
            _connectionAgent = sessionAgent.Connect(objectId, serializer);
        }

        #region Implementation of IConnection

        public void Notify(string eventName, object[] parameters)
        {
            _connectionAgent.Notify(eventName, parameters);
        }

        public object Call(string methodName, object[] parameters)
        {
            return _connectionAgent.Call(_eloquentClient, _contractDescription, methodName, parameters);
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