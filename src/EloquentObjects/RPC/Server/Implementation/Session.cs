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

        public void HandleSessionMessage(SessionMessage sessionMessage, Stream stream)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            //The sessionMessage.ClientHostAddress should always match ClientHostAddress. We do not check it here for optimization purposes.
            
            switch (sessionMessage)
            {
                case HelloSessionMessage helloMessage:
                    HandleHello(helloMessage, stream);
                    break;
                case HeartbeatSessionMessage _:
                    HandleHeartbeat();
                    break;
                case EndpointRequestStartSessionMessage endpointStartMessage:
                    HandleEndpointStartMessage(endpointStartMessage, stream);
                    break;
                case DisconnectSessionMessage disconnectMessage:
                    HandleDisconnect(disconnectMessage);
                    break;
                case TerminateSessionSessionMessage _:
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
        private void HandleHello(HelloSessionMessage helloSessionMessage, Stream stream)
        {
            //Create a new connection or throw an exception if connection already exists
            _connections.AddOrUpdate(helloSessionMessage.ConnectionId, 
                id =>
                {
                    try
                    {
                        return CreateConnection(helloSessionMessage, stream);
                    }
                    catch (Exception e)
                    {
                        WriteException(stream, e);
                        throw;
                    }
                },
                (id, c) =>
                {
                    WriteException(stream, new InvalidOperationException($"Connection with ID {helloSessionMessage.ConnectionId} already exists."));
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
        /// Starts receiving endpoint message that follows the Endpoint Message Start session messages
        /// </summary>
        /// <param name="endpointRequestStartSessionStartMessage"></param>
        /// <param name="stream"></param>
        private void HandleEndpointStartMessage(
            EndpointRequestStartSessionMessage endpointRequestStartSessionStartMessage, Stream stream)
        {
            if (!_connections.TryGetValue(endpointRequestStartSessionStartMessage.ConnectionId, out var connection))
            {
                WriteException(stream, new InvalidOperationException($"Connection with ID {endpointRequestStartSessionStartMessage.ConnectionId} was not established."));
                return;
            }

            connection.Receive(stream, endpointRequestStartSessionStartMessage.ClientHostAddress);
        }

        /// <summary>
        /// Disconnects the endpoint.
        /// </summary>
        private void HandleDisconnect(DisconnectSessionMessage disconnectSessionMessage)
        {
            if (!_connections.TryRemove(disconnectSessionMessage.ConnectionId, out var connection))
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
       
        
        private IConnection CreateConnection(HelloSessionMessage helloSessionMessage,
            Stream writingStream)
        {
            //All callback agents reuse the same output channel
            if (_outputChannel == null)
                _outputChannel = _binding.CreateOutputChannel(ClientHostAddress);
            
            var connection = _endpointHub.ConnectEndpoint(helloSessionMessage.EndpointId, ClientHostAddress, helloSessionMessage.ConnectionId, _outputChannel);

            var helloAck = helloSessionMessage.CreateAck();
            helloAck.Write(writingStream);

            return connection;
        }

        private void WriteException(Stream stream, Exception exception)
        {
            var exceptionMessage = new ExceptionSessionMessage(ClientHostAddress, FaultException.Create(exception));
            exceptionMessage.Write(stream);
        }
    }
}