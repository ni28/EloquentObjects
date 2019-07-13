using System;
using EloquentObjects.RPC.Messages.OneWay;

namespace EloquentObjects.RPC.Server
{
    internal interface IEvent
    {
        void Raise(object[] arguments);
        void Add(ISubscription subscription);
        void UnsubscribeHandler(Action<EventMessage> handler);
    }
}