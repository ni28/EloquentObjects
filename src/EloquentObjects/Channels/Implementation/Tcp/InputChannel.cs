using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using EloquentObjects.Logging;

namespace EloquentObjects.Channels.Implementation.Tcp
{
    internal sealed class InputChannel : IInputChannel
    {
        private readonly IPAddress _ipAddress;
        private readonly int _port;
        private bool _disposed;
        private TcpListener _listener;
        private readonly ILogger _logger;
        private readonly int _sendTimeout;

        public InputChannel(IPAddress ipAddress, int port, int sendTimeout)
        {
            _ipAddress = ipAddress;
            _port = port;
            _sendTimeout = sendTimeout;

            _logger = Logger.Factory.Create(GetType());
            _logger.Info(() => $"Created (ipAddress = {ipAddress}, port = {port})");
        }

        public async void Start()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            if (_listener != null) throw new InvalidOperationException("Input channel was already started");
            _listener = new TcpListener(_ipAddress, _port);
            _listener.Start();

            while (!_disposed)
            {
                try
                {
                    var tcpClient = await _listener.AcceptTcpClientAsync();
                    tcpClient.SendTimeout = _sendTimeout;

                    ThreadPool.QueueUserWorkItem(s => { Process(tcpClient); });
                }
                catch (Exception)
                {
                    // Hide all exceptions for unexpected connection lost with the client
                }
            }
        }

        public event EventHandler<IInputContext> FrameReceived;

        #region IDisposable

        public void Dispose()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            _disposed = true;
            try
            {
                _listener?.Stop();
            }
            catch
            {
                //Hide exception on disposal
            }

            _logger.Info(() => $"Disposed (ipAddress = {_ipAddress}, port = {_port})");
        }

        #endregion

        private void Process(TcpClient tcpClient)
        {
            try
            {
                var networkStream = tcpClient.GetStream();
                using(var bufferedStream = new BufferedStream(networkStream))
                {
                    while (!_disposed)
                    {
                        var frameBytes = bufferedStream.TakeBuffer();
                        var frame = new Frame(frameBytes);
                        var context = new InputContext(bufferedStream, frame);
                        FrameReceived?.Invoke(this, context);
                    }
                }
            }
            catch (Exception)
            {
                // Hide all exceptions for unexpected connection lost with the client
            }
            finally
            {
                if (tcpClient.Connected)
                    tcpClient.Close();
            }
        }
    }
}