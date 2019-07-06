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

            var sessionMessage = SessionMessage.Read(stream);

            if (_disposed)
                return;
    
            switch (sessionMessage)
            {
                case HelloSessionMessage helloMessage:
                    HandleHello(helloMessage, stream);
                    break;
                default:
                    var session = _sessions[sessionMessage.ClientHostAddress];
                    session.HandleSessionMessage(sessionMessage, stream);
                    break;
            }
        }

        private void HandleHello(SessionMessage helloSessionMessage, Stream stream)
        {
            var clientHostAddress = helloSessionMessage.ClientHostAddress;

            var session = _sessions.GetOrAdd(clientHostAddress, address =>
            {
                var s = new Session(_binding, clientHostAddress, _endpointHub);

                s.Terminated += SessionOnTerminated;
                return s;
            });

            session.HandleSessionMessage(helloSessionMessage, stream);
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