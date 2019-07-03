using System;

namespace EloquentObjects.RPC.Client
{
    internal interface IProxy: IDisposable
    {
        event EventHandler<NotifyEventArgs> Notified;
        
        event EventHandler<CallEventArgs> Called;
        
        event EventHandler<SubscriptionEventArgs> EventSubscribed;
        
        event EventHandler<SubscriptionEventArgs> EventUnsubscribed;
    }
}