using System;
using EloquentObjects.Channels;
using EloquentObjects.RPC.Messages.OneWay;

namespace EloquentObjects.RPC.Server.Implementation
{
    internal sealed class Subscription: ISubscription
    {
        private readonly IOutputChannel _outputChannel;
        private readonly IEvent _event;

        public Subscription(string eventName, IHostAddress clientHostAddress,
            IOutputChannel outputChannel, IEvent @event)
        {
            _outputChannel = outputChannel;
            _event = @event;
            EventName = eventName;
            ClientHostAddress = clientHostAddress;
            
            _event.Add(this);
        }
        
        public string EventName { get; }
        public Action<EventMessage> Handler => SendEventToClient;
        
        public IHostAddress ClientHostAddress { get; }
        
        private void SendEventToClient(EventMessage eventMessage)
        {
            _outputChannel.Send(eventMessage);
        }

        #region IDisposable

        public void Dispose()
        {
            _event.UnsubscribeHandler(SendEventToClient);
        }

        #endregion
    }
}