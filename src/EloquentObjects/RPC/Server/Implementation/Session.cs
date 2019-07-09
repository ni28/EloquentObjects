using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using EloquentObjects.Channels;
using EloquentObjects.Logging;
using EloquentObjects.RPC.Messages;
using EloquentObjects.RPC.Messages.Session;

namespace EloquentObjects.RPC.Server.Implementation
{
    internal sealed class Session : ISession
    {
        private readonly ConcurrentDictionary<int, IConnection> _connections = new ConcurrentDictionary<int, IConnection>();
        private readonly IEndpointHub _endpointHub;
        private readonly int _maxHeartBeatLost;
        private readonly IBinding _binding;
        private int _heartBeatLostCounter;
        private Timer _heartbeatTimer;
        private IOutputChannel _outputChannel;
        private readonly ILogger _logger;
        private bool _disposed;

        public Session(IBinding binding, IHostAddress clientHostAddress, IEndpointHub endpointHub)
        {
            _binding = binding;
            _maxHeartBeatLost = binding.MaxHeartBeatLost;
            _endpointHub = endpointHub;
            ClientHostAddress = clientHostAddress;

            //When HeartBeatMs is 0 then no heart beats are listened.
            if (_binding.HeartBeatMs != 0)
                _heartbeatTimer = new Timer(Heartbeat, null, 0, _binding.HeartBeatMs);
            
            _logger = Logger.Factory.Create(GetType());
            _logger.Info(() => $"Created (clientHostAddress = {ClientHostAddress})");
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            _disposed = true;
            foreach (var connection in _connections.Values)
            {
                connection.Dispose();
            }
            _connections.Clear();

            _heartbeatTimer?.Dispose();
            _heartbeatTimer = null;
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

        public void HandleMessage(Message message, Stream stream)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            //The message.ClientHostAddress should always match ClientHostAddress. We do not check it here for optimization purposes.
            
            switch (message)
            {
                case HelloMessage helloMessage:
                    HandleHello(helloMessage, stream);
                    break;
                case HeartbeatMessage _:
                    HandleHeartbeat();
                    break;
                case RequestMessage requestMessage:
                    HandleRequestMessage(requestMessage, stream);
                    break;
                case EventMessage eventMessage:
                    HandleEventMessage(eventMessage, stream);
                    break;
                case DisconnectMessage disconnectMessage:
                    HandleDisconnect(disconnectMessage);
                    break;
                case TerminateMessage _:
                    HandleTerminate();
                    break;
                default:
                    throw new ArgumentException("Unexpected Session Message received");
            }
        }

        #endregion

        /// <summary>
        /// Creates a new connection
        /// </summary>
        private void HandleHello(HelloMessage helloMessage, Stream stream)
        {
            //Create a new connection or throw an exception if connection already exists
            _connections.AddOrUpdate(helloMessage.ConnectionId, 
                id =>
                {
                    try
                    {
                        return CreateConnection(helloMessage, stream);
                    }
                    catch (Exception e)
                    {
                        WriteException(stream, e);
                        throw;
                    }
                },
                (id, c) =>
                {
                    WriteException(stream, new InvalidOperationException($"Connection with ID {helloMessage.ConnectionId} already exists."));
                    return c;
                });
        }

        /// <summary>
        /// Keeps the client alive
        /// </summary>
        private void HandleHeartbeat()
        {
            _heartBeatLostCounter = 0;
        }

        /// <summary>
        /// Redirects handling of the request message to target connection.
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <param name="stream"></param>
        private void HandleRequestMessage(RequestMessage requestMessage, Stream stream)
        {
            if (!_connections.TryGetValue(requestMessage.ConnectionId, out var connection))
            {
                WriteException(stream, new InvalidOperationException($"Connection with ID {requestMessage.ConnectionId} was not established."));
                return;
            }

            connection.HandleRequest(stream, requestMessage);
        }

        /// <summary>
        /// Redirects handling of the event message to target connection.
        /// </summary>
        /// <param name="eventMessage"></param>
        /// <param name="stream"></param>
        private void HandleEventMessage(EventMessage eventMessage, Stream stream)
        {
            if (!_connections.TryGetValue(eventMessage.ConnectionId, out var connection))
            {
                //No exception as the client does not expect responses for events
                return;
            }

            connection.HandleEvent(stream, eventMessage);
        }

        /// <summary>
        /// Disconnects the endpoint.
        /// </summary>
        private void HandleDisconnect(DisconnectMessage disconnectMessage)
        {
            if (!_connections.TryRemove(disconnectMessage.ConnectionId, out var connection))
                return;
            
            connection.Dispose();
        }

        /// <summary>
        /// Terminates the session.
        /// </summary>
        private void HandleTerminate()
        {
            Terminated?.Invoke(this, EventArgs.Empty);
        }
       
        
        private IConnection CreateConnection(HelloMessage helloMessage,
            Stream writingStream)
        {
            //All callback agents reuse the same output channel
            if (_outputChannel == null)
            {
                try
                {
                    _outputChannel = _binding.CreateOutputChannel(ClientHostAddress);
                }
                catch (Exception e)
                {
                    throw new IOException("Connection failed. Client callback channel was not found.", e);
                }

            }
            
            var acknowledged = _endpointHub.TryConnectEndpoint(helloMessage.EndpointId, ClientHostAddress, helloMessage.ConnectionId, _outputChannel, out var connection);

            var helloAck = helloMessage.CreateAck(acknowledged);
            helloAck.Write(writingStream);

            return connection;
        }

        private void WriteException(Stream stream, Exception exception)
        {
            var exceptionMessage = new ExceptionMessage(ClientHostAddress, FaultException.Create(exception));
            exceptionMessage.Write(stream);
        }
    }
}