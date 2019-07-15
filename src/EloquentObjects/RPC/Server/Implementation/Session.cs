using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EloquentObjects.Channels;
using EloquentObjects.Logging;
using EloquentObjects.RPC.Messages;
using EloquentObjects.RPC.Messages.Acknowledged;
using EloquentObjects.RPC.Messages.OneWay;

namespace EloquentObjects.RPC.Server.Implementation
{
    internal sealed class Session : ISession
    {
        private readonly IObjectsRepository _objectsRepository;
        private readonly int _maxHeartBeatLost;
        private int _heartBeatLostCounter;
        private Timer _heartbeatTimer;
        private readonly IOutputChannel _outputChannel;
        private readonly ILogger _logger;
        private bool _disposed;
        private readonly Dictionary<IEvent, ISubscription> _subscriptions = new Dictionary<IEvent, ISubscription>();

        public Session(IBinding binding, IHostAddress clientHostAddress, IObjectsRepository objectsRepository,
            IOutputChannel outputChannel)
        {
            _maxHeartBeatLost = binding.MaxHeartBeatLost;
            _objectsRepository = objectsRepository;
            _outputChannel = outputChannel;
            ClientHostAddress = clientHostAddress;
            
            _objectsRepository.ObjectRemoved += ObjectsRepositoryOnObjectRemoved;

            //When HeartBeatMs is 0 then no heart beats are listened.
            if (binding.HeartBeatMs != 0)
                _heartbeatTimer = new Timer(Heartbeat, null, 0, binding.HeartBeatMs);
            
            _logger = Logger.Factory.Create(GetType());
            _logger.Info(() => $"Created (clientHostAddress = {ClientHostAddress})");
        }
        
        #region Implementation of IDisposable

        public void Dispose()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            _disposed = true;

            _heartbeatTimer?.Dispose();
            _heartbeatTimer = null;
            
            IEnumerable<ISubscription> subscriptions;
            lock (_subscriptions)
            {
                subscriptions = _subscriptions.Values.ToArray();
            }
            foreach (var subscription in subscriptions)
            {
                subscription.Dispose();
            }

            _outputChannel?.Dispose();

            _logger.Info(() => $"Disposed (clientHostAddress = {ClientHostAddress})");
        }

        #endregion

        private void Heartbeat(object state)
        {
            if (_heartbeatTimer == null)
                return;

            _heartBeatLostCounter = Interlocked.Increment(ref _heartBeatLostCounter);

            if (_heartBeatLostCounter > _maxHeartBeatLost) Terminated?.Invoke(this, EventArgs.Empty);
        }

        #region Implementation of ISession

        public IHostAddress ClientHostAddress { get; }

        public event EventHandler Terminated;

        public void HandleMessage(Message message, IInputContext context)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            //The message.ClientHostAddress should always match ClientHostAddress. We do not check it here for optimization purposes.
            switch (message)
            {
                case ConnectMessage connectObjectMessage:
                    HandleConnect(connectObjectMessage, context);
                    break;
                case HeartbeatMessage _:
                    HandleHeartbeat();
                    break;
                case RequestMessage requestMessage:
                    HandleRequestMessage(requestMessage, context);
                    break;
                case NotificationMessage notificationMessage:
                    HandleNotificationMessage(notificationMessage);
                    break;
                case SubscribeEventMessage subscribeEventMessage:
                    HandleSubscribeEventMessage(subscribeEventMessage, context);
                    break;
                case UnsubscribeEventMessage unsubscribeEventMessage:
                    HandleUnsubscribeEventMessage(unsubscribeEventMessage, context);
                    break;
                case TerminateMessage _:
                    HandleTerminate();
                    break;
                default:
                    throw new ArgumentException($"Unexpected Message type received: {message.GetType()}");
            }
        }

        #endregion

        private void HandleConnect(ConnectMessage connectMessage, IInputContext context)
        {
            if (!_objectsRepository.TryGetObject(connectMessage.ObjectId, out _))
            {
                WriteObjectNotFoundError(context, connectMessage.ObjectId);
                return;
            }

            var ackMessage = new AckMessage(connectMessage.ClientHostAddress);
            context.Write(ackMessage.ToFrame());
        }

        /// <summary>
        /// Keeps the client alive
        /// </summary>
        private void HandleHeartbeat()
        {
            _heartBeatLostCounter = 0;
        }

        /// <summary>
        /// Redirects handling of the request message to target object.
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <param name="context"></param>
        private void HandleRequestMessage(RequestMessage requestMessage, IInputContext context)
        {
            if (!_objectsRepository.TryGetObject(requestMessage.ObjectId, out var objectAdapter))
            {
                WriteObjectNotFoundError(context, requestMessage.ObjectId);
                return;
            }

            objectAdapter?.HandleCall(context, requestMessage);
        }

        /// <summary>
        /// Redirects handling of the event message to target object.
        /// </summary>
        /// <param name="notificationMessage"></param>
        private void HandleNotificationMessage(NotificationMessage notificationMessage)
        {
            _objectsRepository.TryGetObject(notificationMessage.ObjectId, out var objectAdapter);
            objectAdapter?.HandleNotification(notificationMessage);
        }
        
        private void HandleSubscribeEventMessage(SubscribeEventMessage subscribeEventMessage, IInputContext context)
        {
            if (!_objectsRepository.TryGetObject(subscribeEventMessage.ObjectId, out var objectAdapter) || objectAdapter == null)
            {
                WriteObjectNotFoundError(context, subscribeEventMessage.ObjectId);
                return;
            }

            if (!objectAdapter.Events.TryGetValue(subscribeEventMessage.EventName, out var ev))
            {
                WriteEventNotFoundError(context, subscribeEventMessage.ObjectId, subscribeEventMessage.EventName);
                return;
            }
            
            var subscription = new Subscription(subscribeEventMessage.ObjectId, subscribeEventMessage.EventName,
                subscribeEventMessage.ClientHostAddress, _outputChannel, ev);

            lock (_subscriptions)
            {
                if (_subscriptions.ContainsKey(ev))
                {
                    WriteEventAlreadySubscribedError(context, subscribeEventMessage.ObjectId, subscribeEventMessage.EventName);
                    return;
                }
                _subscriptions.Add(ev, subscription);
            }

            var ackMessage = new AckMessage(subscribeEventMessage.ClientHostAddress);
            context.Write(ackMessage.ToFrame());
        }


        private void HandleUnsubscribeEventMessage(UnsubscribeEventMessage unsubscribeEventMessage,
            IInputContext context)
        {
            if (!_objectsRepository.TryGetObject(unsubscribeEventMessage.ObjectId, out var objectAdapter) || objectAdapter == null)
            {
                WriteObjectNotFoundError(context, unsubscribeEventMessage.ObjectId);
                return;
            }
            
            if (!objectAdapter.Events.TryGetValue(unsubscribeEventMessage.EventName, out var ev))
            {
                WriteEventNotFoundError(context, unsubscribeEventMessage.ObjectId, unsubscribeEventMessage.EventName);
                return;
            }

            ISubscription subscription;
            lock (_subscriptions)
            {
                if (!_subscriptions.TryGetValue(ev, out subscription))
                {
                    //No exception or error is needed (same as for standard c# events when unsubscribed from a delegate that was not subscribed before).
                    return;
                }

                _subscriptions.Remove(ev);
            }
            
            subscription.Dispose();
            
            var ackMessage = new AckMessage(unsubscribeEventMessage.ClientHostAddress);
            context.Write(ackMessage.ToFrame());
        }

        
        /// <summary>
        /// Terminates the session.
        /// </summary>
        private void HandleTerminate()
        {
            Terminated?.Invoke(this, EventArgs.Empty);
        }
       
        private void WriteObjectNotFoundError(IInputContext context, string objectId)
        {
            var exceptionMessage = new ErrorMessage(ClientHostAddress, ErrorType.ObjectNotFound, $"Object with id '{objectId}' is not hosted on server.");
            context.Write(exceptionMessage.ToFrame());
        }
       
        private void WriteEventNotFoundError(IInputContext context, string objectId, string eventName)
        {
            var exceptionMessage = new ErrorMessage(ClientHostAddress, ErrorType.EventNotFound, $"Event with name {eventName} was not found for object with id '{objectId}'.");
            context.Write(exceptionMessage.ToFrame());
        }
       
        private void WriteEventAlreadySubscribedError(IInputContext context, string objectId, string eventName)
        {
            var exceptionMessage = new ErrorMessage(ClientHostAddress, ErrorType.EventAlreadySubscribed, $"Event with name {eventName} for object with id '{objectId}' is already subscribed.");
            context.Write(exceptionMessage.ToFrame());
        }
        
        private void ObjectsRepositoryOnObjectRemoved(object sender, string objectId)
        {
            lock (_subscriptions)
            {
                //TODO: Test
                foreach (var pair in _subscriptions.ToArray())
                {
                    var subscription = pair.Value;
                    if (subscription.ObjectId == objectId)
                    {
                        subscription.Dispose();
                        _subscriptions.Remove(pair.Key);
                    }
                }
            }
            
        }
    }
}