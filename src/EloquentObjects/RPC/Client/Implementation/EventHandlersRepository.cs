using System;
using System.Collections.Generic;
using System.Linq;
using EloquentObjects.Contracts;

namespace EloquentObjects.RPC.Client.Implementation
{
    internal sealed class EventHandlersRepository : IEventHandlersRepository
    {
        private readonly IContractDescription _contractDescription;

        private readonly List<RemoteEventSubscription> _subscriptions = new List<RemoteEventSubscription>();
        private bool _disposed;
        private readonly object _outerProxy;

        public EventHandlersRepository(IContractDescription contractDescription, object outerProxy)
        {
            _contractDescription = contractDescription;
            _outerProxy = outerProxy;
        }
        
        #region Implementation of ICallback

        public void Subscribe(string eventName, Delegate handler)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(EventHandlersRepository));
            }
            
            lock (_subscriptions)
            {
                _subscriptions.Add(new RemoteEventSubscription(eventName, handler));
            }
        }

        public void Unsubscribe(string eventName, Delegate handler)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(EventHandlersRepository));
            }

            lock (_subscriptions)
            {
                var subscriptionToRemove = _subscriptions.First(s =>
                    s.EventName == eventName && s.Handler == handler);
                _subscriptions.Remove(subscriptionToRemove);
            }
        }

        public void HandleEvent(string eventName, object[] arguments)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(EventHandlersRepository));
            }

            IEnumerable<RemoteEventSubscription> subscriptions;

            //Set the sender = proxy object for standard events
            var eventDescription = _contractDescription.Events.First(e => e.Name == eventName);
            if (eventDescription.IsStandardEvent)
            {
                arguments[0] = _outerProxy;
            }
            
            lock (_subscriptions)
            {
                subscriptions = _subscriptions.Where(s => s.EventName == eventName).ToArray();
            }
            
            foreach (var subscription in subscriptions) subscription.Handler.DynamicInvoke(arguments);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _disposed = true;
            _subscriptions.Clear();
        }

        #endregion
    }
}