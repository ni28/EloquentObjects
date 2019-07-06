using System;
using System.Collections.Generic;
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

        public IConnectionAgent Connect(string endpointId, ISerializer serializer)
        {
            var connectionId = Interlocked.Increment(ref _lastConnectionId);

            //Send hello to ensure that endpoint exists
            var helloMessage = new HelloSessionMessage(_clientHostAddress, endpointId, connectionId);

            SessionMessage response;

            try
            {
                using (var context = _outputChannel.BeginWriteRead())
                {
                    helloMessage.Write(context.Stream);
                    response = SessionMessage.Read(context.Stream);
                }
            }
            catch (Exception e)
            {
                throw new IOException("Connection failed. Check that server is still alive", e);
            }

            switch (response)
            {
                case ExceptionSessionMessage exceptionMessage:
                    throw exceptionMessage.Exception;
                case HelloAckSessionMessage helloAckMessage:
                    if (!helloAckMessage.Acknowledged)
                        throw new KeyNotFoundException($"No objects with ID {endpointId} are hosted on server");
                    return CreateConnectionAgent(connectionId, endpointId, serializer);
                default:
                    throw new IOException("Unexpected failure. Connection is not acknowledged by the server.");
            }
        }

        public event EventHandler<EndpointMessageReadyEventArgs> EndpointMessageReady;

        private IConnectionAgent CreateConnectionAgent(int connectionId, string endpointId, ISerializer serializer)
        {
            //Start sending heartbeats if not started yet
            //When HeartBeatMs is 0 then no heart beats are sent.
            lock (_heartbeatTimerLock)
            {
                if (_heartbeatTimer == null && _binding.HeartBeatMs != 0)
                    _heartbeatTimer = new Timer(Heartbeat, null, 0, _binding.HeartBeatMs);
            }

            return new ConnectionAgent(connectionId, endpointId, _outputChannel, _clientHostAddress, serializer);
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
                    EndpointMessageReady?.Invoke(this, new EndpointMessageReadyEventArgs(endpointStartMessage.ConnectionId, stream));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
            try
            {
                var terminateSessionMessage = new TerminateSessionSessionMessage(_clientHostAddress);

                using (var context = _outputChannel.BeginWriteRead())
                {
                    terminateSessionMessage.Write(context.Stream);
                }
            }
            catch (IOException)
            {
                //Hide IOException when disposing a client while the server is not alive.
            }
        }
    }
}