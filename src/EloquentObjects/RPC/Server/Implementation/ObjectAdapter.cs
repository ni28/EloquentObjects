using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using EloquentObjects.Channels;
using EloquentObjects.Contracts;
using EloquentObjects.RPC.Messages.Session;
using EloquentObjects.Serialization;

namespace EloquentObjects.RPC.Server.Implementation
{
    internal sealed class ObjectAdapter : IObjectAdapter
    {
        private readonly List<IConnection> _connections = new List<IConnection>();
        private readonly IContractDescription _contractDescription;
        private readonly ISerializer _serializer;
        private readonly List<HostedEvent> _hostedEvents = new List<HostedEvent>();
        private readonly object _serviceInstance;
        private readonly SynchronizationContext _synchronizationContext;
        private bool _disposed;

        public ObjectAdapter(IContractDescription contractDescription,
            ISerializer serializer,
            SynchronizationContext synchronizationContext,
            object serviceInstance)
        {
            _contractDescription = contractDescription;
            _serializer = serializer;
            _synchronizationContext = synchronizationContext;
            _serviceInstance = serviceInstance;

            foreach (var eventDescription in _contractDescription.Events)
            {
                var handler = CreateHandler(eventDescription.Event, args => SendEventToAllClients(eventDescription.Name, eventDescription.IsStandardEvent, args));
                eventDescription.Event.AddEventHandler(serviceInstance, handler);
                _hostedEvents.Add(new HostedEvent(eventDescription.Event, handler));
            }
        }

        private static Delegate CreateHandler(EventInfo evt, Action<object[]> d)
        {
            var handlerType = evt.EventHandlerType;
            var eventInvokeMethod = handlerType.GetMethod("Invoke");
            if (eventInvokeMethod == null)
            {
                throw new InvalidOperationException($"Failed getting Invoke method for event {evt.Name}");
            }

            var actionInvokeMethod = d.GetType().GetMethod("Invoke");
            if (actionInvokeMethod == null)
            {
                throw new InvalidOperationException("Failed getting Invoke method for given action");
            }
            
            //lambda: (T1 p1, T2 p2, ...) => d(new object[]{ (object)p1, (object)p2, ... })
            var parameters = eventInvokeMethod.GetParameters().Select(p => Expression.Parameter(p.ParameterType, "x")).ToArray();
            var boxedParameters = parameters.Select(p => Expression.Convert(p, typeof(object)));
            var arr = Expression.NewArrayInit(typeof(object), boxedParameters);
            var body = Expression.Call(Expression.Constant(d), actionInvokeMethod, arr);
            var lambda = Expression.Lambda(body, parameters);
            var del = Delegate.CreateDelegate(handlerType, lambda.Compile(), "Invoke", false);

            if (del == null)
            {
                throw new InvalidOperationException("Failed creating event handler");
            }

            return del;
        }
        
        private void SendEventToAllClients(string eventName, bool isStandardEvent, params object[] parameters)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ObjectAdapter));
            var connections = new List<IConnection>();

            lock (_connections)
            {
                connections.AddRange(_connections);
            }

            if (isStandardEvent)
            {
                //Do not need to serialize the sender for standard events (that have EventHandler and EventHandler<T> types);
                parameters[0] = null;
            }

            foreach (var callbackAgent in connections)
            {
                callbackAgent.Notify(eventName, parameters);
            }
        }

        #region Implementation of IObjectAdapter

        public IConnection Connect(string objectId,
            IHostAddress clientHostAddress, int connectionId,
            IOutputChannel outputChannel)
        {
            var connection = new Connection(objectId, clientHostAddress, connectionId, outputChannel, _serializer);

            connection.RequestReceived += ConnectionOnRequestReceived;
            connection.EventReceived += ConnectionOnEventReceived;
            connection.Disconnected += ConnectionOnDisconnected;

            lock (_connections)
            {
                _connections.Add(connection);
            }
            
            return connection;
        }

        #endregion

        private void ConnectionOnDisconnected(object sender, EventArgs e)
        {
            var connection = (IConnection) sender;
            connection.RequestReceived -= ConnectionOnRequestReceived;
            connection.EventReceived -= ConnectionOnEventReceived;
            connection.Disconnected -= ConnectionOnDisconnected;

            lock (_connections)
            {
                _connections.Remove(connection);
            }
        }

        private void ConnectionOnRequestReceived(IInputContext context, IHostAddress clientHostAddress, CallInfo callInfo)
        {
            try
            {
                HandleRequest(clientHostAddress, context, callInfo.OperationName, callInfo.Parameters);
            }
            catch (Exception e)
            {
                WriteException(context, e, clientHostAddress);
            }
        }

        private void ConnectionOnEventReceived(IHostAddress clientHostAddress, CallInfo callInfo)
        {
            HandleEvent(callInfo.OperationName, callInfo.Parameters);
        }

        public void HandleEvent(string eventName, object[] arguments)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ObjectAdapter));

            var operationDescription = _contractDescription.GetOperationDescription(eventName, arguments);

            if (_synchronizationContext != null)
            {
                _synchronizationContext.Post(
                    s => { operationDescription.Method.Invoke(_serviceInstance, arguments); },
                    null);

                return;
            }

            try
            {
                operationDescription.Method.Invoke(_serviceInstance, arguments);
            }
            catch (Exception)
            {
                //Hide exception for one-way calls as client does not expect an answer
            }
        }

        public void HandleRequest(IHostAddress clientHostAddress, IInputContext context, string methodName,
            object[] parameters)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ObjectAdapter));

            var operationDescription = _contractDescription.GetOperationDescription(methodName, parameters);

            object result;
            try
            {
                result = _synchronizationContext != null
                    ? CallInSyncContext(operationDescription, parameters)
                    : operationDescription.Method.Invoke(_serviceInstance, parameters.ToArray());
            }
            catch (Exception e)
            {
                //Send exception back to client
                WriteException(context, e.InnerException ?? e, clientHostAddress);
                return;
            }

            if (result is IEloquent eloquent)
            {
                var payload = _serializer.Serialize(eloquent.Info);
                var responseMessage = new EloquentObjectMessage(clientHostAddress, eloquent.ObjectId, payload);
                context.Write(responseMessage.ToFrame());
            }
            else
            {
                var payload = _serializer.Serialize(result);
                var responseMessage = new ResponseMessage(clientHostAddress, payload);
                context.Write(responseMessage.ToFrame());
            }
        }


        private object CallInSyncContext(IMethodDescription methodDescription, object[] parameters)
        {
            object result = null;
            Exception exception = null;
            var resetEvent = new AutoResetEvent(false);

            _synchronizationContext.Post(s =>
            {
                try
                {
                    result = methodDescription.Method.Invoke(_serviceInstance, parameters);
                }
                catch (Exception e)
                {
                    exception = e;
                }

                resetEvent.Set();
            }, null);

            resetEvent.WaitOne();

            if (exception != null)
                throw exception;

            return result;
        }
        
        #region Implementation of IDisposable

        public void Dispose()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ObjectAdapter));

            _disposed = true;
            foreach (var hostedEvent in _hostedEvents)
                hostedEvent.EventInfo.RemoveEventHandler(_serviceInstance, hostedEvent.Handler);
        }

        #endregion
        
        private void WriteException(IInputContext context, Exception exception, IHostAddress clientHostAddress)
        {
            var exceptionMessage = new ExceptionMessage(clientHostAddress, FaultException.Create(exception));
            context.Write(exceptionMessage.ToFrame());
        }
    }
}