using System;

namespace EloquentObjects.RPC.Client
{
    internal sealed class SubscriptionEventArgs: EventArgs
    {
        public SubscriptionEventArgs(string eventName, Delegate handler)
        {
            EventName = eventName;
            Handler = handler;
        }

        public string EventName { get; }
        public Delegate Handler { get; }
    }
}