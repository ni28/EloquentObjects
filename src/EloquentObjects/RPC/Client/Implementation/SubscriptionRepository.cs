using System;
using System.Collections.Generic;
using System.Linq;
using EloquentObjects.Serialization;

namespace EloquentObjects.RPC.Client.Implementation
{
    internal sealed class SubscriptionRepository
    {
        private readonly bool _hasSender;
        private readonly ISerializer _serializer;
        private readonly List<RemoteEventSubscription> _subscriptions = new List<RemoteEventSubscription>();
            
        public SubscriptionRepository(bool hasSender, ISerializer serializer)
        {
            _hasSender = hasSender;
            _serializer = serializer;
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

        public void Raise(byte[] eventData)
        {
            IEnumerable<RemoteEventSubscription> subscriptions;
            lock (_subscriptions)
            {
                subscriptions = _subscriptions.ToArray();
            }

            var callInfo = _serializer.DeserializeCall(eventData);
            
            foreach (var subscription in subscriptions)
            {
                //Create a copy to avoid dependency between handlers
                var args = callInfo.Parameters.ToArray();
                
                //Set the sender = proxy object for standard events
                if (_hasSender)
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