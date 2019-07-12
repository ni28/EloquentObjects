using System;
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

        void Subscribe(SubscribeEventMessage subscribeEventMessage, Action<EventMessage> sendEventToClient);
        void Unsubscribe(UnsubscribeEventMessage unsubscribeEventMessage, Action<EventMessage> sendEventToClient);
    }
}