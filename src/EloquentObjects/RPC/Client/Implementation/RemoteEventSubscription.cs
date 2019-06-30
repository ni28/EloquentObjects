using System;

namespace EloquentObjects.RPC.Client.Implementation
{
    internal struct RemoteEventSubscription
    {
        public RemoteEventSubscription(string eventName, Delegate handler)
        {
            EventName = eventName;
            Handler = handler;
        }

        public string EventName { get; }
        
        public Delegate Handler { get; }
    }
}