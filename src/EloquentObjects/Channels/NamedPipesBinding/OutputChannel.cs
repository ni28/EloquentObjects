using System;
using System.IO.Pipes;
using System.Threading;
using EloquentObjects.Channels.Implementation;
using EloquentObjects.Logging;

namespace EloquentObjects.Channels.NamedPipesBinding
{
    internal sealed class OutputChannel : IOutputChannel
    {
        private readonly string _pipeName;
        private readonly int _sendTimeout;
        private NamedPipeClientStream _stream;
        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(true);
        private readonly ILogger _logger;
        private bool _disposed;

        public OutputChannel(string pipeName, int sendTimeout)
        {
            _pipeName = pipeName;
            _sendTimeout = sendTimeout;
            
            _logger = Logger.Factory.Create(GetType());
            _logger.Info(() => $"Created (pipeName = {_pipeName})");

            //TODO: what is server name?
            _stream = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

            _stream.Connect();

        }

        #region IDisposable

        public void Dispose()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            _disposed = true;
            
            _stream.Close();
            _stream.Dispose();
            _resetEvent.Dispose();

            _logger.Info(() => $"Disposed (pipeName = {_pipeName})");
        }

        #endregion


        #region Implementation of IOutputChannel

        public IOutputChannelContext BeginWriteRead()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            _resetEvent.WaitOne();

            return new OutputChannelContext(_stream, _resetEvent);
        }

        #endregion

    }
}