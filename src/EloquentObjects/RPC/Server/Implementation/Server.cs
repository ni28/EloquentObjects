using System;
using System.Collections.Concurrent;
using System.IO;
using EloquentObjects.Channels;
using EloquentObjects.Logging;
using EloquentObjects.RPC.Messages;
using EloquentObjects.RPC.Messages.Session;

namespace EloquentObjects.RPC.Server.Implementation
{
    internal sealed class Server : IServer
    {
        private readonly IBinding _binding;
        private readonly IEndpointHub _endpointHub;
        private readonly IInputChannel _inputChannel;

        private readonly ConcurrentDictionary<IHostAddress, ISession> _sessions = new ConcurrentDictionary<IHostAddress, ISession>();
        private bool _disposed;
        private readonly ILogger _logger;

        public Server(IBinding binding, IInputChannel inputChannel, IEndpointHub endpointHub)
        {
            _binding = binding;
            _inputChannel = inputChannel;
            _endpointHub = endpointHub;

            _inputChannel.MessageReady += InputChannelOnMessageReady;

            _inputChannel.Start();
            
            _logger = Logger.Factory.Create(GetType());
            _logger.Info(() => "Created)");
        }

        #region IDisposable

        public void Dispose()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            _disposed = true;
            
            _inputChannel.MessageReady -= InputChannelOnMessageReady;

            foreach (var session in _sessions.Values)
            {
                session.Terminated -= SessionOnTerminated;
                session.Dispose();
            }

            _sessions.Clear();

            _logger.Info(() => "Disposed)");
        }

        #endregion

        private void InputChannelOnMessageReady(object sender, Stream stream)
        {
            if (_disposed)
                return;

            var message = Message.Read(stream);

            if (_disposed)
                return;
    
            switch (message)
            {
                case HelloMessage helloMessage:
                    HandleHello(helloMessage, stream);
                    break;
                default:
                    var session = _sessions[message.ClientHostAddress];
                    session.HandleMessage(message, stream);
                    break;
            }
        }

        private void HandleHello(Message helloMessage, Stream stream)
        {
            var clientHostAddress = helloMessage.ClientHostAddress;

            var session = _sessions.GetOrAdd(clientHostAddress, address =>
            {
                var s = new Session(_binding, clientHostAddress, _endpointHub);

                s.Terminated += SessionOnTerminated;
                return s;
            });

            session.HandleMessage(helloMessage, stream);
        }

        private void SessionOnTerminated(object sender, EventArgs e)
        {
            var session = (ISession) sender;
            if (!_sessions.TryRemove(session.ClientHostAddress, out _))
            {
                return;
            }
            session.Terminated -= SessionOnTerminated;
            session.Dispose();
        }
    }
}