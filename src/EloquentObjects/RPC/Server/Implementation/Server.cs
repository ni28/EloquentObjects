using System;
using System.Collections.Concurrent;
using System.IO;
using EloquentObjects.Channels;
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

        public Server(IBinding binding, IInputChannel inputChannel, IEndpointHub endpointHub)
        {
            _binding = binding;
            _inputChannel = inputChannel;
            _endpointHub = endpointHub;

            _inputChannel.MessageReady += InputChannelOnMessageReady;

            _inputChannel.Start();
        }

        #region IDisposable

        public void Dispose()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Server));

            _disposed = true;
            _inputChannel.MessageReady -= InputChannelOnMessageReady;
            _inputChannel.Dispose();


            foreach (var session in _sessions.Values)
            {
                session.Terminated -= SessionOnTerminated;
                session.Dispose();
            }

            _sessions.Clear();
        }

        #endregion

        private void InputChannelOnMessageReady(object sender, Stream stream)
        {
            if (_disposed)
                return;

            try
            {
                var sessionMessage = SessionMessage.Read(_binding, stream);
        
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
            catch (Exception)
            {
                //Hide all exceptions and continue receiving new messages
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