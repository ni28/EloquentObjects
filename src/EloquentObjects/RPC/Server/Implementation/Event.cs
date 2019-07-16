using System;
using System.Collections.Generic;
using System.Linq;
using EloquentObjects.RPC.Messages.OneWay;
using EloquentObjects.Serialization;

namespace EloquentObjects.RPC.Server.Implementation
{
    internal sealed class Event : IEvent
    {
        private readonly string _objectId;
        private readonly string _eventName;
        private readonly ISerializer _serializer;
        private readonly IObjectsRepository _objectsRepository;
        private readonly Dictionary<ISubscription, int> _subscriptions = new Dictionary<ISubscription, int>();
            
        public Event(string objectId, string eventName, ISerializer serializer,
            IObjectsRepository objectsRepository)
        {
            _objectId = objectId;
            _eventName = eventName;
            _serializer = serializer;
            _objectsRepository = objectsRepository;
        }
        
        public void Raise(object[] arguments)
        {
            IEnumerable<ISubscription> subscriptions;
            lock (_subscriptions)
            {
                subscriptions = _subscriptions.Keys.ToArray();
            }
                
            var payload = Payload.Create(arguments, o => _objectsRepository.TryGetObjectId(o, out var objectId) ? objectId : null);

            foreach (var subscription in subscriptions)
            {
                //TODO: optimization is needed to avoid serialization for each subscriber
                var eventMessage = payload.CreateEventMessage(_serializer, subscription.ClientHostAddress, _objectId, _eventName);
                subscription.Handler.DynamicInvoke(eventMessage);
            }
        }

        public void Add(ISubscription subscription)
        {
            lock (_subscriptions)
            {
                if (_subscriptions.TryGetValue(subscription, out var numOfSubscriptions))
                {
                    _subscriptions[subscription] = numOfSubscriptions + 1;
                }
                else
                {
                    _subscriptions.Add(subscription, 1);
                }
            }
        }
            
        public void UnsubscribeHandler(Action<EventMessage> handler)
        {
            lock (_subscriptions)
            {
                var subscription = _subscriptions.Keys.First(s => s.Handler == handler);

                var numOfSubscriptions = _subscriptions[subscription];
                if (numOfSubscriptions == 1)
                {
                    _subscriptions.Remove(subscription);
                }
                else
                {
                    _subscriptions[subscription] = numOfSubscriptions - 1;
                }
            }   
        }
    }
}