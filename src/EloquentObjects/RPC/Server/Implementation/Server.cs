using System;
using System.Collections.Generic;
using EloquentObjects.Channels;
using EloquentObjects.Logging;
using EloquentObjects.RPC.Messages;
using EloquentObjects.RPC.Messages.Acknowledged;
using EloquentObjects.RPC.Messages.OneWay;

namespace EloquentObjects.RPC.Server.Implementation
{
    internal sealed class Server : IServer
    {
        private readonly IBinding _binding;
        private readonly IObjectsRepository _objectsRepository;
        private readonly IInputChannel _inputChannel;

        private readonly Dictionary<IHostAddress, ISession> _sessions = new Dictionary<IHostAddress, ISession>();
        private bool _disposed;
        private readonly ILogger _logger;

        public Server(IBinding binding, IInputChannel inputChannel, IObjectsRepository objectsRepository)
        {
            _binding = binding;
            _inputChannel = inputChannel;
            _objectsRepository = objectsRepository;

            _inputChannel.FrameReceived += InputChannelOnFrameReceived;

            _inputChannel.Start();
            
            _logger = Logger.Factory.Create(GetType());
            _logger.Info(() => "Created");
        }

        #region IDisposable

        public void Dispose()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            _disposed = true;
            
            _inputChannel.FrameReceived -= InputChannelOnFrameReceived;

            lock (_sessions)
            {
                foreach (var session in _sessions.Values)
                {
                    session.Terminated -= SessionOnTerminated;
                    session.Dispose();
                }

                _sessions.Clear();
            }

            _logger.Info(() => "Disposed");
        }

        #endregion

        private void InputChannelOnFrameReceived(object sender, IInputContext context)
        {
            if (_disposed)
                return;

            var message = Message.Create(context.Frame, "Server");

            switch (message)
            {
                case HelloMessage helloMessage:
                    HandleHello(helloMessage, context);
                    break;
                default:
                    var session = _sessions[message.ClientHostAddress];
                    session.HandleMessage(message, context);
                    break;
            }
        }

        private void HandleHello(Message helloMessage, IInputContext context)
        {
            var clientHostAddress = helloMessage.ClientHostAddress;

            lock (_sessions)
            {
                if (_sessions.ContainsKey(clientHostAddress))
                    return;
                
                var outputChannel = _binding.CreateOutputChannel(clientHostAddress);

                var session = new Session(_binding, clientHostAddress, _objectsRepository, outputChannel);
                
                session.Terminated += SessionOnTerminated;
                
                var helloAck = new AckMessage(clientHostAddress);
                context.Write(helloAck.ToFrame());
                
                _sessions.Add(clientHostAddress, session);
            }
        }

        private void SessionOnTerminated(object sender, EventArgs e)
        {
            lock (_sessions)
            {
                var session = (ISession) sender;
                if (!_sessions.Remove(session.ClientHostAddress))
                {
                    return;
                }
                session.Terminated -= SessionOnTerminated;
                session.Dispose();
            }
        }
    }
}