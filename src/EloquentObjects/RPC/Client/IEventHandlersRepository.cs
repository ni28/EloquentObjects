using System;

namespace EloquentObjects.RPC.Client
{
    internal interface IEventHandlersRepository: IDisposable
    {
        void Subscribe(string eventName, Delegate handler);
        void Unsubscribe(string eventName, Delegate handler);
        void HandleEvent(string eventName, object[] arguments);
    }
}