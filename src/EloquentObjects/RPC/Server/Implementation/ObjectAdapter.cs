using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using EloquentObjects.Channels;
using EloquentObjects.Contracts;
using EloquentObjects.RPC.Messages.Acknowledged;
using EloquentObjects.RPC.Messages.OneWay;
using EloquentObjects.Serialization;

namespace EloquentObjects.RPC.Server.Implementation
{
    internal sealed class ObjectAdapter : IObjectAdapter
    {
        private readonly IContractDescription _contractDescription;
        private readonly ISerializer _serializer;
        private readonly List<HostedEvent> _hostedEvents = new List<HostedEvent>();
        private IObjectsRepository _objectsRepository;
        private readonly SynchronizationContext _synchronizationContext;
        private readonly Dictionary<string, SubscriptionRepository> _events = new Dictionary<string, SubscriptionRepository>();
        private bool _disposed;

        public ObjectAdapter(string objectId, IContractDescription contractDescription,
            ISerializer serializer,
            SynchronizationContext synchronizationContext,
            object objectToHost, IObjectsRepository objectsRepository)
        {
            _contractDescription = contractDescription;
            _serializer = serializer;
            _synchronizationContext = synchronizationContext;
            Object = objectToHost;
            _objectsRepository = objectsRepository;

            foreach (var eventDescription in _contractDescription.Events)
            {
                var handler = CreateHandler(eventDescription.Event, args => SendEventToAllClients(eventDescription.Name, eventDescription.IsStandardEvent, args));
                eventDescription.Event.AddEventHandler(objectToHost, handler);
                _hostedEvents.Add(new HostedEvent(eventDescription.Event, handler));
                _events.Add(eventDescription.Name, new SubscriptionRepository(objectId, eventDescription.Name, eventDescription.IsStandardEvent, serializer));
            }
        }

        
        #region Implementation of IObjectAdapter

        public object Object { get; }

        public void HandleCall(IInputContext context, RequestMessage requestMessage)
        {
            try
            {
                var callInfo = _serializer.DeserializeCall(requestMessage.Payload);
                HandleRequest(requestMessage.ClientHostAddress, context, callInfo.OperationName, callInfo.Parameters);
            }
            catch (Exception e)
            {
                WriteException(context, e, requestMessage.ClientHostAddress);
            }
        }

        public void HandleNotification(NotificationMessage notificationMessage)
        {
            try
            {
                var callInfo = _serializer.DeserializeCall(notificationMessage.Payload);
                HandleEvent(callInfo.OperationName, callInfo.Parameters);
            }
            catch (Exception)
            {
                //Hide exception for one-way calls as client does not expect an answer
            }
        }

        public void Subscribe(SubscribeEventMessage subscribeEventMessage, Action<EventMessage> sendEventToClient)
        {
            lock (_events)
            {
                _events[subscribeEventMessage.EventName].Subscribe(sendEventToClient, subscribeEventMessage.ClientHostAddress);
            }
        }

        public void Unsubscribe(UnsubscribeEventMessage unsubscribeEventMessage, Action<EventMessage> sendEventToClient)
        {
            lock (_events)
            {
                _events[unsubscribeEventMessage.EventName].Unsubscribe(sendEventToClient);
            }
        }

        #endregion
        
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

            if (isStandardEvent)
            {
                //Do not need to serialize the sender for standard events (that have EventHandler and EventHandler<T> types);
                parameters[0] = null;
            }

            lock (_events)
            {
                _events[eventName].Raise(parameters);
            }
        }

        private void HandleEvent(string eventName, object[] arguments)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ObjectAdapter));

            var operationDescription = _contractDescription.GetOperationDescription(eventName, arguments);

            if (_synchronizationContext != null)
            {
                _synchronizationContext.Post(
                    s => { operationDescription.Method.Invoke(Object, arguments); },
                    null);

                return;
            }

            operationDescription.Method.Invoke(Object, arguments);
        }

        private void HandleRequest(IHostAddress clientHostAddress, IInputContext context, string methodName,
            object[] parameters)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ObjectAdapter));

            var operationDescription = _contractDescription.GetOperationDescription(methodName, parameters);

            object result;
            try
            {
                result = _synchronizationContext != null
                    ? CallInSyncContext(operationDescription, parameters)
                    : operationDescription.Method.Invoke(Object, parameters.ToArray());
            }
            catch (Exception e)
            {
                //Send exception back to client
                WriteException(context, e.InnerException ?? e, clientHostAddress);
                return;
            }

            if (_objectsRepository.TryGetObjectId(result, out var objectId))
            {
                var payload = _serializer.Serialize(objectId);
                var responseMessage = new EloquentObjectMessage(clientHostAddress, objectId, payload);
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
                    result = methodDescription.Method.Invoke(Object, parameters);
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
                hostedEvent.EventInfo.RemoveEventHandler(Object, hostedEvent.Handler);

            _objectsRepository = null;
        }

        #endregion
        
        private void WriteException(IInputContext context, Exception exception, IHostAddress clientHostAddress)
        {
            var exceptionMessage = new ExceptionMessage(clientHostAddress, FaultException.Create(exception));
            context.Write(exceptionMessage.ToFrame());
        }

    }
}