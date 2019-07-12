using System;

namespace EloquentObjects.RPC.Client
{
    internal interface IConnection
    {
        void Notify(string eventName, object[] parameters);
        
        object Call(string methodName, object[] parameters);
        
        void Subscribe(string eventName, Delegate handler);
        
        void Unsubscribe(string eventName, Delegate handler);
    }
}