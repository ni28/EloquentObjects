using System;
using System.IO;
using EloquentObjects.Channels;
using EloquentObjects.RPC.Messages.Session;
using EloquentObjects.Serialization;

namespace EloquentObjects.RPC.Server
{
    internal interface IConnection : IDisposable
    {
        void Notify(string eventName, object[] arguments);
        
        void HandleRequest(Stream stream, RequestMessage requestMessage);
        void HandleEvent(Stream stream, EventMessage eventMessage);

        event EventHandler Disconnected;
        
        event Action<IHostAddress, CallInfo> EventReceived;
        event Action<Stream, IHostAddress, CallInfo> RequestReceived;

    }
}