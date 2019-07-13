using System;
using System.IO;
using System.Threading;
using EloquentObjects.Channels;
using EloquentObjects.Logging;
using EloquentObjects.RPC.Messages;
using EloquentObjects.RPC.Messages.OneWay;

namespace EloquentObjects.RPC.Client.Implementation
{
    internal sealed class SessionAgent : ISessionAgent
    {
        private readonly IHostAddress _clientHostAddress;
        private readonly IEventHandlersRepository _eventHandlersRepository;

        private readonly object _heartbeatTimerLock = new object();
        private readonly IInputChannel _inputChannel;
        private readonly IOutputChannel _outputChannel;
        private bool _disposed;
        private Timer _heartbeatTimer;
        private readonly ILogger _logger;

        public SessionAgent(IBinding binding, IInputChannel inputChannel, IOutputChannel outputChannel,
            IHostAddress clientHostAddress, IEventHandlersRepository eventHandlersRepository)
        {
            _inputChannel = inputChannel;
            _outputChannel = outputChannel;
            _clientHostAddress = clientHostAddress;
            _eventHandlersRepository = eventHandlersRepository;

            _inputChannel.FrameReceived += InputChannelOnFrameReceived;
            
            //Start sending heartbeats. When HeartBeatMs is 0 then no heart beats are sent.
            lock (_heartbeatTimerLock)
            {
                if (binding.HeartBeatMs != 0)
                    _heartbeatTimer = new Timer(Heartbeat, null, 0, binding.HeartBeatMs);
            }
            
            _logger = Logger.Factory.Create(GetType());
            _logger.Info(() => $"Created (clientHostAddress = {_clientHostAddress})");
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
            _inputChannel.FrameReceived -= InputChannelOnFrameReceived;

            _logger.Info(() => $"Disposed (clientHostAddress = {_clientHostAddress})");
        }

        #endregion

        private void InputChannelOnFrameReceived(object sender, IInputContext context)
        {
            if (_disposed)
                return;

            var message = Message.Create(context.Frame);

            switch (message)
            {
                case EventMessage eventMessage:
                    _eventHandlersRepository.HandleEvent(eventMessage);
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

            var heartbeatMessage = new HeartbeatMessage(_clientHostAddress);
            _outputChannel.Send(heartbeatMessage, "Client");
        }

        private void Terminate()
        {
            try
            {
                var terminateMessage = new TerminateMessage(_clientHostAddress);
                _outputChannel.Send(terminateMessage);
            }
            catch (IOException)
            {
                //Hide IOException when disposing a client while the server is not alive.
            }
        }
    }
}