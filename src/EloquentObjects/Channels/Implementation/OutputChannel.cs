using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using EloquentObjects.Logging;

namespace EloquentObjects.Channels.Implementation
{
    internal sealed class OutputChannel : IOutputChannel
    {
        private readonly IPAddress _ipAddress;
        private readonly int _port;
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(true);
        private readonly ILogger _logger;
        private readonly BufferedStream _bufferedStream;

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

            _stream = _client.GetStream();
            _bufferedStream = new BufferedStream(_stream);

            _logger = Logger.Factory.Create(GetType());
            _logger.Info(() => $"Created (ipAddress = {ipAddress}, port = {port})");

        }

        #region IDisposable

        public void Dispose()
        {
            _bufferedStream.Dispose();
            _stream.Dispose();
            _client.Close();
            ((IDisposable)_client).Dispose();
            _resetEvent.Dispose();

            _logger.Info(() => $"Disposed (ipAddress = {_ipAddress}, port = {_port})");
        }

        #endregion


        #region Implementation of IOutputChannel

        public IOutputChannelContext BeginWriteRead()
        {
            _resetEvent.WaitOne();
            return new OutputChannelContext(_bufferedStream, _resetEvent);
        }

        #endregion
    }
}