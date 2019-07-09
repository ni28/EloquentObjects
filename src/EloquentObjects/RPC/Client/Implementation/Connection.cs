using System;
using EloquentObjects.Contracts;
using EloquentObjects.Logging;
using EloquentObjects.RPC.Messages.Session;
using EloquentObjects.Serialization;

namespace EloquentObjects.RPC.Client.Implementation
{
    internal sealed class Connection<T> : IConnection<T>
    {
        private readonly IProxy _innerProxy;
        private readonly EventHandlersRepository _eventHandlersRepository;
        private readonly ISessionAgent _sessionAgent;
        private readonly IContractDescription _contractDescription;
        private readonly EloquentClient _eloquentClient;
        private readonly T _outerProxy;
        private bool _disposed;
        private readonly IConnectionAgent _connectionAgent;
        private readonly ILogger _logger;

        internal Connection(string objectId,
            IProxy innerProxy,
            T outerProxy,
            EventHandlersRepository eventHandlersRepository,
            ISessionAgent sessionAgent,
            ISerializer serializer,
            IContractDescription contractDescription,
            EloquentClient eloquentClient)
        {
            _innerProxy = innerProxy;
            _eventHandlersRepository = eventHandlersRepository;
            _sessionAgent = sessionAgent;
            _contractDescription = contractDescription;
            _eloquentClient = eloquentClient;
            _outerProxy = outerProxy;

            _connectionAgent = sessionAgent.Connect(objectId, serializer);

            _sessionAgent.EventReceived += SessionAgentOnEventReceived;

            _innerProxy.Notified += InnerProxyOnNotified;
            _innerProxy.Called += InnerProxyOnCalled;
            _innerProxy.EventSubscribed += InnerProxyOnEventSubscribed;
            _innerProxy.EventUnsubscribed += InnerProxyOnEventUnsubscribed;
            
            _connectionAgent.EventReceived += ConnectionAgentOnEventReceived;
            
            _logger = Logger.Factory.Create(GetType());
            _logger.Info(() => "Created");
        }

        private void SessionAgentOnEventReceived(object sender, EventMessage e)
        {
            if (e.ConnectionId != _connectionAgent.ConnectionId)
                return;
            
            _connectionAgent.HandleEvent(e);
        }

        private void InnerProxyOnNotified(object sender, NotifyEventArgs e)
        {
            _connectionAgent.Notify(e.EventName, e.Parameters);
        }

        private void InnerProxyOnCalled(object sender, CallEventArgs e)
        {
            e.ReturnValue = _connectionAgent.Call(_eloquentClient, _contractDescription, e.MethodName, e.Parameters);
        }

        private void InnerProxyOnEventSubscribed(object sender, SubscriptionEventArgs e)
        {
            _eventHandlersRepository.Subscribe(e.EventName, e.Handler);
        }

        private void InnerProxyOnEventUnsubscribed(object sender, SubscriptionEventArgs e)
        {
            _eventHandlersRepository.Unsubscribe(e.EventName, e.Handler);
        }
        
        private void ConnectionAgentOnEventReceived(object sender, NotifyEventArgs e)
        {
            _eventHandlersRepository.HandleEvent(e.EventName, e.Parameters);
        }
        
        public T Object
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(Connection<T>));
                return _outerProxy;
            }
        }

        #region IDisposable

        public void Dispose()
        {
            _disposed = true;
            
            _sessionAgent.EventReceived -= SessionAgentOnEventReceived;

            _innerProxy.Notified -= InnerProxyOnNotified;
            _innerProxy.Called -= InnerProxyOnCalled;
            _innerProxy.EventSubscribed -= InnerProxyOnEventSubscribed;
            _innerProxy.EventUnsubscribed -= InnerProxyOnEventUnsubscribed;
            
            _connectionAgent.EventReceived -= ConnectionAgentOnEventReceived;
            
            _connectionAgent.Dispose();
            _eventHandlersRepository.Dispose();
            _innerProxy.Dispose();

            _logger.Info(() => "Disposed");
        }

        #endregion
    }
}