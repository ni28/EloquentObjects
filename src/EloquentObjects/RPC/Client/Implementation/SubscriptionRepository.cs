using System;
using System.Collections.Generic;
using System.Linq;
using EloquentObjects.Contracts;
using EloquentObjects.RPC.Messages.OneWay;
using EloquentObjects.Serialization;

namespace EloquentObjects.RPC.Client.Implementation
{
    internal sealed class SubscriptionRepository
    {
        private readonly IEventDescription _eventDescription;
        private readonly ISerializer _serializer;
        private readonly EloquentClient _eloquentClient;
        private readonly List<RemoteEventSubscription> _subscriptions = new List<RemoteEventSubscription>();
            
        public SubscriptionRepository(IEventDescription eventDescription, ISerializer serializer, EloquentClient eloquentClient)
        {
            _eventDescription = eventDescription;
            _serializer = serializer;
            _eloquentClient = eloquentClient;
        }

        public bool IsEmpty
        {
            get
            {
                lock (_subscriptions)
                {
                    return _subscriptions.Count == 0;
                }
            }
        }

        public void Raise(EventMessage eventMessage)
        {
            IEnumerable<RemoteEventSubscription> subscriptions;
            lock (_subscriptions)
            {
                subscriptions = _subscriptions.ToArray();
            }

            var serializedParameters = _serializer.Deserialize(eventMessage.SerializedParameters);
            var payload = new Payload(serializedParameters, eventMessage.ObjectIds, eventMessage.Selector);
            
            //TODO: Refactoring is needed
            //TODO: add null and range checks
            var invokeMethod = _eventDescription.Event.EventHandlerType.GetMethod("Invoke");
            var invokeMethodParameters = invokeMethod.GetParameters();
            var eventParameters = invokeMethodParameters.Select(p => p.ParameterType).ToArray();
            
            var parameters = payload.ToParametersNoCheck((objectId, paramIndex) =>
            {
                if (_eventDescription.IsStandardEvent && paramIndex == 0)
                {
                    return null;
                }
                var type = eventParameters[paramIndex];
                return _eloquentClient.Connect(type, objectId);
            });

            foreach (var subscription in subscriptions)
            {
                //Create a copy to avoid dependency between handlers
                var args = parameters.ToArray();
                                
                //Set the sender = proxy object for standard events
                if (_eventDescription.IsStandardEvent)
                {
                    args[0] = subscription.Proxy;
                }
                
                //TODO: test
                //Execute in a try-catch so that if there is an exception then other subscriptions are still handled
                try
                {
                    subscription.Handler.DynamicInvoke(args);
                }
                catch (Exception)
                {
                    //Hide exception
                }
            }
        }

        public void Subscribe(Delegate handler, object proxy)
        {
            lock (_subscriptions)
            {
                _subscriptions.Add(new RemoteEventSubscription(handler, proxy));
            }
        }
            
        public void Unsubscribe(Delegate handler)
        {
            lock (_subscriptions)
            {
                try
                {
                    var subscription = _subscriptions.First(s => s.Handler == handler);
                    _subscriptions.Remove(subscription);
                }
                catch (Exception)
                {
                    //Hide exception if there is no subscription for the handler - just like standard object events
                }
            }   
        }
    }
}