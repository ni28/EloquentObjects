using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using EloquentObjects.Channels;
using EloquentObjects.Contracts;
using EloquentObjects.RPC.Messages;
using EloquentObjects.RPC.Messages.Endpoint;
using EloquentObjects.RPC.Messages.Session;
using EloquentObjects.Serialization;

namespace EloquentObjects.RPC.Server.Implementation
{
    internal sealed class ServiceEndpoint : IEndpoint
    {
        private readonly List<IConnection> _connections = new List<IConnection>();
        private readonly IContractDescription _contractDescription;
        private readonly ISerializer _serializer;
        private readonly List<HostedEvent> _hostedEvents = new List<HostedEvent>();
        private readonly object _serviceInstance;
        private readonly SynchronizationContext _synchronizationContext;
        private bool _disposed;

        public ServiceEndpoint(IContractDescription contractDescription,
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
                var handler = CreateHandler(eventDescription.Event, args => SendEventToAllClients(eventDescription.Name, args));
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
                throw new InvalidOperationException($"Failed getting Invoke method for given action");
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
        
        private void SendEventToAllClients(string eventName, params object[] parameters)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ServiceEndpoint));
            var connections = new List<IConnection>();

            lock (_connections)
            {
                connections.AddRange(_connections);
            }

            foreach (var callbackAgent in connections)
            {
                callbackAgent.Notify(eventName, parameters);
            }
        }

        #region Implementation of IEndpoint

        public IConnection Connect(string endpointId,
            IHostAddress clientHostAddress, int connectionId,
            IOutputChannel outputChannel)
        {
            var connection = new Connection(endpointId, clientHostAddress, connectionId, outputChannel, _serializer);

            connection.MessageReady += ConnectionOnMessageReady;
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
            connection.MessageReady -= ConnectionOnMessageReady;
            connection.Disconnected -= ConnectionOnDisconnected;

            lock (_connections)
            {
                _connections.Remove(connection);
            }
        }

        private void ConnectionOnMessageReady(Stream stream, IHostAddress clientHostAddress)
        {
            ReceiveAndHandleEndpointMessage(stream, clientHostAddress);
        }

        private void ReceiveAndHandleEndpointMessage(Stream stream, IHostAddress clientHostAddress)
        {
            var message = EndpointMessage.Read(stream, _serializer);
            switch (message)
            {
                case EventEndpointMessage eventEndpointMessage:
                    HandleEvent(eventEndpointMessage.EventName, eventEndpointMessage.Arguments);
                    break;
                case RequestEndpointMessage requestEndpointMessage:
                    try
                    {
                        var response = HandleRequest(requestEndpointMessage.MethodName, requestEndpointMessage.Parameters);

                        var startEndpointMessage = new EndpointResponseStartSessionMessage(clientHostAddress);
                        startEndpointMessage.Write(stream);
                        var responseMessage = new ResponseEndpointMessage(response);
                        responseMessage.Write(stream, _serializer);
                    }
                    catch (Exception e)
                    {
                        WriteException(stream, e, clientHostAddress);
                    }

                    break;
                default:
                    throw new InvalidOperationException("Unexpected Endpoint message type");
            }
        }

        private void HandleEvent(string eventName, object[] arguments)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ServiceEndpoint));

            var operationDescription = _contractDescription.GetOperationDescription(eventName, arguments);

            if (_synchronizationContext != null)
            {
                _synchronizationContext.Post(
                    s => { operationDescription.Method.Invoke(_serviceInstance, arguments); },
                    null);

                return;
            }

            operationDescription.Method.Invoke(_serviceInstance, arguments);
        }

        private object HandleRequest(string methodName, object[] parameters)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ServiceEndpoint));

            var operationDescription = _contractDescription.GetOperationDescription(methodName, parameters);

            if (_synchronizationContext != null) return CallInSyncContext(operationDescription, parameters);

            return operationDescription.Method.Invoke(_serviceInstance, parameters.ToArray());
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
            if (_disposed) throw new ObjectDisposedException(nameof(ServiceEndpoint));

            _disposed = true;
            foreach (var hostedEvent in _hostedEvents)
                hostedEvent.EventInfo.RemoveEventHandler(_serviceInstance, hostedEvent.Handler);
        }

        #endregion
        
        private void WriteException(Stream stream, Exception exception, IHostAddress clientHostAddress)
        {
            var exceptionMessage = new ExceptionSessionMessage(clientHostAddress, FaultException.Create(exception));
            exceptionMessage.Write(stream);
        }
    }
}