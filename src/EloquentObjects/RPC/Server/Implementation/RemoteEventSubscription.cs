using System;
using EloquentObjects.Channels;
using EloquentObjects.RPC.Messages.OneWay;

namespace EloquentObjects.RPC.Server.Implementation
{
    internal struct RemoteEventSubscription
    {
        public RemoteEventSubscription(Action<EventMessage> handler, IHostAddress clientHostAddress)
        {
            Handler = handler;
            ClientHostAddress = clientHostAddress;
        }

        public Action<EventMessage> Handler { get; }
        
        public IHostAddress ClientHostAddress { get; }
    }
}