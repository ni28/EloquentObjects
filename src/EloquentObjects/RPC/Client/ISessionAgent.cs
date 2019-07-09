using System;
using EloquentObjects.RPC.Messages.Session;
using EloquentObjects.Serialization;

namespace EloquentObjects.RPC.Client
{
    internal interface ISessionAgent : IDisposable
    {
        IConnectionAgent Connect(string endpointId, ISerializer serializer);

        event EventHandler<EventMessage> EventReceived;
    }
}