using System;
using EloquentObjects.Channels;
using EloquentObjects.RPC.Messages.OneWay;

namespace EloquentObjects.RPC.Server
{
    internal interface ISubscription: IDisposable
    {
        string EventName { get; }
        Action<EventMessage> Handler { get; }
        
        IHostAddress ClientHostAddress { get; }
    }
}