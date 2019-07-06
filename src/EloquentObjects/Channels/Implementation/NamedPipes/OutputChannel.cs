using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using EloquentObjects.Logging;

namespace EloquentObjects.Channels.Implementation.NamedPipes
{
    internal sealed class OutputChannel : IOutputChannel
    {
        private readonly string _pipeName;
        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(true);
        private readonly ILogger _logger;
        private bool _disposed;
        private readonly BufferedStream _bufferedStream;

        public OutputChannel(string pipeName, int sendTimeout)
        {
            _pipeName = pipeName;

            _logger = Logger.Factory.Create(GetType());
            _logger.Info(() => $"Created (pipeName = {_pipeName})");

            //TODO: what is server name?
            var stream = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            stream.Connect(sendTimeout);
            _bufferedStream = new BufferedStream(stream);
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

            return new OutputChannelContext(_bufferedStream, _resetEvent);
        }

        #endregion

    }
}