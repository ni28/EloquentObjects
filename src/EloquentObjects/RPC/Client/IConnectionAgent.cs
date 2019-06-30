using System;
using System.IO;

namespace EloquentObjects.RPC.Client
{
    internal interface IConnectionAgent : IDisposable
    {
        int ConnectionId { get; }
        void Notify(string eventName, object[] arguments);

        object Call(string methodName, object[] parameters);

        event EventHandler Disconnected;
        void ReceiveAndHandleEndpointMessage(Stream stream);
    }
}