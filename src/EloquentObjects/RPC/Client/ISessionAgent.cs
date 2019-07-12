using System;
using EloquentObjects.RPC.Messages.OneWay;
using EloquentObjects.Serialization;

namespace EloquentObjects.RPC.Client
{
    internal interface ISessionAgent : IDisposable
    {
        IConnectionAgent Connect(string objectId, ISerializer serializer);
    }
}