using System;
using System.Reflection;

namespace EloquentObjects.RPC.Server.Implementation
{
    internal struct HostedEvent
    {
        public HostedEvent(EventInfo eventInfo, Delegate handler)
        {
            EventInfo = eventInfo;
            Handler = handler;
        }

        public EventInfo EventInfo { get; }
        public Delegate Handler { get; }
    }
}