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
        private readonly bool _hasSender;
        private readonly ISerializer _serializer;
        private readonly Dictionary<ISubscription, int> _subscriptions = new Dictionary<ISubscription, int>();
            
        public Event(string objectId, string eventName, bool hasSender, ISerializer serializer)
        {
            _objectId = objectId;
            _eventName = eventName;
            _hasSender = hasSender;
            _serializer = serializer;
        }
        
        public void Raise(object[] arguments)
        {
            IEnumerable<ISubscription> subscriptions;
            lock (_subscriptions)
            {
                subscriptions = _subscriptions.Keys.ToArray();
            }
                
            foreach (var subscription in subscriptions)
            {
                //Create a copy to avoid dependency between handlers
                var args = arguments.ToArray();
                
                //Set the sender = proxy object for standard events
                if (_hasSender)
                {
                    args[0] = null;
                }

                var bytes = _serializer.Serialize(args);

                var eventMessage = new EventMessage(subscription.ClientHostAddress, _objectId, _eventName, bytes);
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