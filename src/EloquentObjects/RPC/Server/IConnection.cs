using System;
using EloquentObjects.Channels;
using EloquentObjects.RPC.Messages.Session;
using EloquentObjects.Serialization;

namespace EloquentObjects.RPC.Server
{
    internal interface IConnection : IDisposable
    {
        void Notify(string eventName, object[] arguments);
        
        void HandleRequest(IInputContext context, RequestMessage requestMessage);
        void HandleEvent(EventMessage eventMessage);

        event EventHandler Disconnected;
        
        event Action<IHostAddress, CallInfo> EventReceived;
        event Action<IInputContext, IHostAddress, CallInfo> RequestReceived;

    }
}