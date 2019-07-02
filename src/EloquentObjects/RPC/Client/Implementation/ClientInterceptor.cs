using System;
using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;
using EloquentObjects.Contracts;
using EloquentObjects.Logging;
using EloquentObjects.Serialization;

namespace EloquentObjects.RPC.Client.Implementation
{
    internal sealed class ClientInterceptor : IInterceptor, ICallback, IDisposable
    {
        private readonly IConnectionAgent _connectionAgent;
        private readonly string _endpointId;
        private readonly IContractDescription _contractDescription;

        private readonly List<RemoteEventSubscription> _subscriptions = new List<RemoteEventSubscription>();

        private bool _disposed;
        private readonly ILogger _logger;

        public ClientInterceptor(string endpointId, ISessionAgent sessionAgent,
            IContractDescription contractDescription, ISerializer serializer)
        {
            _endpointId = endpointId;
            _contractDescription = contractDescription;

            _connectionAgent = sessionAgent.Connect(endpointId, this, serializer);
            
            _logger = Logger.Factory.Create(GetType());
            _logger.Info(() => $"Created (endpointId = {_endpointId}, contract = {_contractDescription})");
        }

        #region Implementation of ICallback

        public void HandleEvent(string eventName, object[] arguments)
        {
            IEnumerable<RemoteEventSubscription> subscriptions;
            lock (_subscriptions)
            {
                subscriptions = _subscriptions.Where(s => s.EventName == eventName).ToArray();
            }

            foreach (var subscription in subscriptions) subscription.Handler.DynamicInvoke(arguments);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
            _disposed = true;

            _subscriptions.Clear();
            _connectionAgent.Dispose();

            _logger.Info(() => $"Disposed (endpointId = {_endpointId}, contract = {_contractDescription})");
        }

        #endregion

        #region Implementation of IInterceptor

        public void Intercept(IInvocation invocation)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ClientInterceptor));

            if (invocation.Method.Name == nameof(Dispose) && invocation.Arguments.Length == 0)
            {
                Dispose();
                return;
            }

            if (invocation.Method.IsSpecialName)
            {
                HandleSpecialName(invocation);
            }
            else
            {
                var operation = _contractDescription.GetOperationDescription(invocation.Method.Name, invocation.Arguments);
                
                if (operation.Method.ReturnType == typeof(void) && operation.IsOneWay)
                {
                    Notify(operation.Name, invocation.Arguments);
                }
                else
                {
                    invocation.ReturnValue = Call(operation.Name, invocation.Arguments);
                }
            }
        }

        private void HandleSpecialName(IInvocation invocation)
        {
            //Get property value
            if (invocation.Method.Name.StartsWith("get_"))
            {
                invocation.ReturnValue = Call(invocation.Method.Name, new object[]{});
                return;
            }
            //Set property value
            if (invocation.Method.Name.StartsWith("set_"))
            {
                var operation = _contractDescription.GetOperationDescription(invocation.Method.Name, invocation.Arguments);

                if (operation.IsOneWay)
                {
                    //This will hide exceptions
                    Notify(invocation.Method.Name, invocation.Arguments);
                }
                else
                {
                    //This will allow exception to be thrown on client if any
                    Call(invocation.Method.Name, invocation.Arguments);
                }
                return;
            }
            //Subscribe event
            if (invocation.Method.Name.StartsWith("add_"))
            {
                var eventName = invocation.Method.Name.Substring(4);
                lock (_subscriptions)
                {
                    _subscriptions.Add(new RemoteEventSubscription(eventName, (Delegate) invocation.Arguments[0]));
                }
                return;
            }

            //Unsubscribe event
            if (invocation.Method.Name.StartsWith("remove_"))
            {
                var eventName = invocation.Method.Name.Substring(7);
                lock (_subscriptions)
                {
                    _subscriptions.Remove(new RemoteEventSubscription(eventName, (Delegate) invocation.Arguments[0]));
                }
                return;
            }
            
            throw new NotSupportedException($"Special method is not supported: {invocation.Method.Name}");
        }

        #endregion
        
        private void Notify(string name, object[] parameters)
        {
            _connectionAgent.Notify(name, parameters);
        }

        private object Call(string name, object[] parameters)
        {
            return _connectionAgent.Call(name, parameters);
        }
    }
}