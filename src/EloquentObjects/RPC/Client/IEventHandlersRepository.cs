using System;
using EloquentObjects.Contracts;
using EloquentObjects.RPC.Messages.OneWay;
using EloquentObjects.Serialization;

namespace EloquentObjects.RPC.Client
{
    internal interface IEventHandlersRepository: IDisposable
    {
        void Subscribe(string objectId, IEventDescription eventDescription, ISerializer serializer, Delegate handler,
            object proxy);
        void Unsubscribe(string objectId, string eventName, Delegate handler);
        void HandleEvent(EventMessage eventMessage);
    }
}