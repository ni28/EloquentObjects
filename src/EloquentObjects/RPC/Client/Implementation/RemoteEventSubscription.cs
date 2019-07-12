using System;

namespace EloquentObjects.RPC.Client.Implementation
{
    internal struct RemoteEventSubscription
    {
        public RemoteEventSubscription(Delegate handler, object proxy)
        {
            ObjectId = null;
            EventName = null;
            Handler = handler;
            Proxy = proxy;
        }

        //TODO: remove
        public string ObjectId { get; }
        public string EventName { get; }
        
        public Delegate Handler { get; }
        
        public object Proxy { get; }
    }
}