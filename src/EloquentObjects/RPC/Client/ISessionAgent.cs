using System;
using EloquentObjects.Serialization;

namespace EloquentObjects.RPC.Client
{
    internal interface ISessionAgent : IDisposable
    {
        IConnectionAgent Connect(string endpointId, ICallback callback, ISerializer serializer);
    }
}