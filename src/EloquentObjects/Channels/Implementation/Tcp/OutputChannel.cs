using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using EloquentObjects.Logging;

namespace EloquentObjects.Channels.Implementation.Tcp
{
    internal sealed class OutputChannel : IOutputChannel
    {
        private readonly IPAddress _ipAddress;
        private readonly int _port;
        private readonly TcpClient _client;
        private readonly AutoResetEvent _contextCanBeTaken = new AutoResetEvent(true);
        private readonly ILogger _logger;
        private readonly BufferedStream _bufferedStream;
        private bool _disposed;

        public OutputChannel(IPAddress ipAddress, int port, int sendTimeout, int receiveTimeout)
        {
            _ipAddress = ipAddress;
            _port = port;
            _client = new TcpClient
            {
                SendTimeout = sendTimeout,
                ReceiveTimeout = receiveTimeout
            };

            _client.Connect(ipAddress, port);

            var stream = _client.GetStream();
            _bufferedStream = new BufferedStream(stream);

            _logger = Logger.Factory.Create(GetType());
            _logger.Info(() => $"Created (ipAddress = {ipAddress}, port = {port})");

        }

        #region IDisposable

        public void Dispose()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            _disposed = true;

            try
            {
                _bufferedStream.Dispose();
            }
            catch (IOException)
            {
                //Hide exceptions when disposing a broken stream (e.g. when server dead).
            }

            try
            {
                ((IDisposable)_client).Dispose();
            }
            catch (IOException)
            {
                //Hide exceptions when disposing a broken stream (e.g. when server dead).
            }

            _contextCanBeTaken.Set();
            _contextCanBeTaken.Dispose();

            _logger.Info(() => $"Disposed (ipAddress = {_ipAddress}, port = {_port})");
        }

        #endregion


        #region Implementation of IOutputChannel

        public IOutputChannelContext BeginReadWrite()
        {
            _contextCanBeTaken.WaitOne();
            return new OutputChannelContext(_bufferedStream, _contextCanBeTaken);
        }

        #endregion
    }
}