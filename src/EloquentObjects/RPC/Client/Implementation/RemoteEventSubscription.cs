using System;

namespace EloquentObjects.RPC.Client.Implementation
{
    internal struct RemoteEventSubscription
    {
        public RemoteEventSubscription(Delegate handler, object proxy)
        {
            Handler = handler;
            Proxy = proxy;
        }
        
        public Delegate Handler { get; }
        
        public object Proxy { get; }
    }
}