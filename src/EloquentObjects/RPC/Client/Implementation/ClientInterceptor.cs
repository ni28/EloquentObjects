using System;
using Castle.DynamicProxy;
using EloquentObjects.Contracts;
using EloquentObjects.Logging;

namespace EloquentObjects.RPC.Client.Implementation
{
    internal sealed class ClientInterceptor : IInterceptor, IProxy
    {
        private readonly IContractDescription _contractDescription;
        
        private bool _disposed;
        private readonly ILogger _logger;

        public ClientInterceptor(IContractDescription contractDescription)
        {
            _contractDescription = contractDescription;

            _logger = Logger.Factory.Create(GetType());
            _logger.Info(() => $"Created (contract = {_contractDescription})");
        }

        #region IDisposable

        public void Dispose()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
            _disposed = true;

            _logger.Info(() => $"Disposed (contract = {_contractDescription})");
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
                EventSubscribed?.Invoke(this, new SubscriptionEventArgs(eventName, (Delegate) invocation.Arguments[0]));
                return;
            }

            //Unsubscribe event
            if (invocation.Method.Name.StartsWith("remove_"))
            {
                var eventName = invocation.Method.Name.Substring(7);
                EventUnsubscribed?.Invoke(this, new SubscriptionEventArgs(eventName, (Delegate) invocation.Arguments[0]));
                return;
            }
            
            throw new NotSupportedException($"Special method is not supported: {invocation.Method.Name}");
        }

        #endregion
        
        private void Notify(string name, object[] parameters)
        {
            Notified?.Invoke(this, new NotifyEventArgs(name, parameters));
        }

        private object Call(string name, object[] parameters)
        {
            var args = new CallEventArgs(name, parameters);
            Called?.Invoke(this, args);
            return args.ReturnValue;
        }

        #region Implementation of IProxy

        public event EventHandler<NotifyEventArgs> Notified;
        public event EventHandler<CallEventArgs> Called;
        public event EventHandler<SubscriptionEventArgs> EventSubscribed;
        public event EventHandler<SubscriptionEventArgs> EventUnsubscribed;

        #endregion
    }
}