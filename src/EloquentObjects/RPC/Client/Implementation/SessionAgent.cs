using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using EloquentObjects.Channels;
using EloquentObjects.Logging;
using EloquentObjects.RPC.Messages;
using EloquentObjects.RPC.Messages.Session;
using EloquentObjects.Serialization;

namespace EloquentObjects.RPC.Client.Implementation
{
    internal sealed class SessionAgent : ISessionAgent
    {
        private readonly IBinding _binding;
        private readonly IHostAddress _clientHostAddress;

        //Key is a connection ID
        private readonly ConcurrentDictionary<int, IConnectionAgent> _connections = new ConcurrentDictionary<int, IConnectionAgent>();
        private readonly object _heartbeatTimerLock = new object();
        private readonly IInputChannel _inputChannel;
        private readonly IOutputChannel _outputChannel;
        private bool _disposed;
        private Timer _heartbeatTimer;
        private int _lastConnectionId;
        private readonly ILogger _logger;

        public SessionAgent(IBinding binding, IInputChannel inputChannel, IOutputChannel outputChannel,
            IHostAddress clientHostAddress)
        {
            _binding = binding;
            _inputChannel = inputChannel;
            _outputChannel = outputChannel;
            _clientHostAddress = clientHostAddress;

            _inputChannel.MessageReady += InputChannelOnMessageReady;
            _inputChannel.Start();
            
            _logger = Logger.Factory.Create(GetType());
            _logger.Info(() => $"Created (clientHostAddress = {_clientHostAddress})");
        }

        public IConnectionAgent Connect(string endpointId, ICallback callback, ISerializer serializer)
        {
            var connectionId = Interlocked.Increment(ref _lastConnectionId);

            //Send hello to ensure that endpoint exists
            var helloMessage = new HelloSessionMessage(_clientHostAddress, endpointId, connectionId);

            SessionMessage response;
            using (var context = _outputChannel.BeginWriteRead())
            {
                helloMessage.Write(context.Stream);
                response = SessionMessage.Read(context.Stream);
            }

            switch (response)
            {
                case ExceptionSessionMessage exceptionMessage:
                    throw exceptionMessage.Exception;
                case HelloAckSessionMessage _:
                    return CreateConnectionAgent(connectionId, endpointId, callback, serializer);
                default:
                    throw new IOException("Unexpected failure. Connection is not acknowledged by the server.");
            }

        }

        private IConnectionAgent CreateConnectionAgent(int connectionId, string endpointId, ICallback callback,
            ISerializer serializer)
        {
            var connectionAgent = _connections.AddOrUpdate(connectionId,
                id =>
                {
                    
                    var c = new ConnectionAgent(connectionId, callback, endpointId, _outputChannel,
                        _clientHostAddress, serializer, _binding);
                    
                    c.Disconnected += ConnectionAgentOnDisconnected;
                    
                    return c;
                },
                (id, c) => throw new InvalidOperationException($"Connection with ID {id} already exists"));

            //Start sending heartbeats if not started yet
            //When HeartBeatMs is 0 then no heart beats are listened.
            lock (_heartbeatTimerLock)
            {
                if (_heartbeatTimer == null && _binding.HeartBeatMs != 0)
                    _heartbeatTimer = new Timer(Heartbeat, null, 0, _binding.HeartBeatMs);
            }

            return connectionAgent;            
        }

        private void ConnectionAgentOnDisconnected(object sender, EventArgs e)
        {
            var connectionAgent = (IConnectionAgent) sender;
            connectionAgent.Disconnected -= ConnectionAgentOnDisconnected;
            _connections.TryRemove(connectionAgent.ConnectionId, out connectionAgent);
        }


        #region IDisposable

        public void Dispose()
        {
            lock (_heartbeatTimerLock)
            {
                _heartbeatTimer?.Dispose();
                _heartbeatTimer = null;
            }

            Terminate();

            _disposed = true;
            _inputChannel.MessageReady -= InputChannelOnMessageReady;

            foreach (var connectionAgent in _connections.Values)
            {
                connectionAgent.Disconnected -= ConnectionAgentOnDisconnected;
            }
            _connections.Clear();

            _logger.Info(() => $"Disposed (clientHostAddress = {_clientHostAddress})");
        }

        #endregion

        private void InputChannelOnMessageReady(object sender, Stream stream)
        {
            if (_disposed)
                return;

            var message = SessionMessage.Read(stream);

            switch (message)
            {
                case EndpointRequestStartSessionMessage endpointStartMessage:
                    HandleEndpointStartMessage(endpointStartMessage, stream);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleEndpointStartMessage(EndpointRequestStartSessionMessage endpointRequestStartSessionStartMessage, Stream stream)
        {
            if (!_connections.TryGetValue(endpointRequestStartSessionStartMessage.ConnectionId, out var connection))
            {
                //No profit to raise exception as this is running in input channel thread
                return;
            }
            
            connection.ReceiveAndHandleEndpointMessage(stream);
        }

        private void Heartbeat(object state)
        {
            lock (_heartbeatTimerLock)
            {
                if (_heartbeatTimer == null)
                    return;
            }

            var heartbeatMessage = new HeartbeatSessionMessage(_clientHostAddress);
            using (var context = _outputChannel.BeginWriteRead())
            {
                heartbeatMessage.Write(context.Stream);
            }
        }

        private void Terminate()
        {
            var terminateSessionMessage = new TerminateSessionSessionMessage(_clientHostAddress);

            using (var context = _outputChannel.BeginWriteRead())
            {
                terminateSessionMessage.Write(context.Stream);
            }
        }
    }
}