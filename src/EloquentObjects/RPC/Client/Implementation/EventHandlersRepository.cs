using System;
using System.Collections.Generic;
using EloquentObjects.Contracts;
using EloquentObjects.RPC.Messages.OneWay;
using EloquentObjects.Serialization;
using JetBrains.Annotations;

namespace EloquentObjects.RPC.Client.Implementation
{
    internal sealed class EventHandlersRepository : IEventHandlersRepository
    {
        private struct EventId
        {
            [UsedImplicitly]
            public string ObjectId;

            [UsedImplicitly]
            public string EventName;
        }
        
        
        private readonly Dictionary<EventId, SubscriptionRepository> _events = new Dictionary<EventId, SubscriptionRepository>();
        private bool _disposed;


        #region Implementation of ICallback

        public void Subscribe(string objectId, IEventDescription eventDescription, ISerializer serializer,
            Delegate handler, object proxy)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(EventHandlersRepository));
            }
            
            var eventId = new EventId
            {
                ObjectId = objectId,
                EventName = eventDescription.Name
            };

            lock (_events)
            {
                if (!_events.TryGetValue(eventId, out var subscriptionRepository))
                {
                    subscriptionRepository = new SubscriptionRepository(eventDescription.IsStandardEvent, serializer);
                    _events.Add(eventId, subscriptionRepository);
                }
                
                subscriptionRepository.Subscribe(handler, proxy);
            }
        }

        public void Unsubscribe(string objectId, string eventName, Delegate handler)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(EventHandlersRepository));
            }

            var eventId = new EventId
            {
                ObjectId = objectId,
                EventName = eventName
            };

            lock (_events)
            {
                var subscriptionRepository = _events[eventId];
                subscriptionRepository.Unsubscribe(handler);

                if (subscriptionRepository.IsEmpty)
                {
                    _events.Remove(eventId);
                }
            }
        }

        public void HandleEvent(EventMessage eventMessage)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(EventHandlersRepository));
            }

            var eventId = new EventId
            {
                ObjectId = eventMessage.ObjectId,
                EventName = eventMessage.EventName
            };

            SubscriptionRepository subscriptionRepository;
            lock (_events)
            {
                subscriptionRepository = _events[eventId];
            }
            
            subscriptionRepository.Raise(eventMessage.Payload);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _disposed = true;
            _events.Clear();
        }

        #endregion
    }
}