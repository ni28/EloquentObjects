using System;
using System.IO;
using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Server
{
    internal interface IConnection : IDisposable
    {
        void Notify(string eventName, object[] arguments);
        
        void Receive(Stream stream, IHostAddress clientHostAddress);

        event EventHandler Disconnected;
        event Action<Stream, IHostAddress> MessageReady;

    }
}