using System;
using System.IO;
using System.IO.Pipes;
using EloquentObjects.Logging;

namespace EloquentObjects.Channels.Implementation.NamedPipes
{
    internal sealed class InputChannel : IInputChannel
    {
        private readonly string _pipeName;
        private readonly ILogger _logger;
        private bool _disposed;
        private NamedPipeServerStream _stream;

        public InputChannel(string pipeName)
        {
            _pipeName = pipeName;
            
            _logger = Logger.Factory.Create(GetType());
            _logger.Info(() => $"Created (pipeName = {_pipeName})");
        }
        
        #region Implementation of IDisposable

        public void Dispose()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            _disposed = true;

            try
            {
                _stream?.Dispose();
            }
            catch (Exception)
            {
                //Hide exception if the stream is broken already
            }
            
            _logger.Info(() => $"Disposed (pipeName = {_pipeName})");
        }

        #endregion

        #region Implementation of IInputChannel

        public void Start()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            CreateStream();
        }

        private void CreateStream()
        {
            _stream = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            _stream.BeginWaitForConnection(WaitForConnectionCallback, _stream);
        }

        private void WaitForConnectionCallback(IAsyncResult ar)
        {
            if (_disposed)
                return;
            var stream = (NamedPipeServerStream)ar.AsyncState;
            stream.EndWaitForConnection(ar);
            _stream = null;

            CreateStream();

            try
            {
                using (var bufferedStream = new BufferedStream(stream))
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
        }

        public event EventHandler<IInputContext> FrameReceived;

        #endregion
    }
}