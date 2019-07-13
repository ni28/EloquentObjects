using System;
using System.Collections.Generic;
using EloquentObjects.Channels;
using EloquentObjects.RPC.Messages.Acknowledged;
using EloquentObjects.RPC.Messages.OneWay;

namespace EloquentObjects.RPC.Server
{
    internal interface IObjectAdapter : IDisposable
    {
        object Object { get; }
        void HandleCall(IInputContext context, RequestMessage requestMessage);
        void HandleNotification(NotificationMessage notificationMessage);
        IReadOnlyDictionary<string, IEvent> Events { get; }
    }
}