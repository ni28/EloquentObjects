using System;
using System.IO;
using System.Threading;

namespace EloquentObjects.Channels.Implementation
{
    internal sealed class OutputChannelContext : IOutputChannelContext
    {
        private readonly Stream _stream;
        private readonly AutoResetEvent _contextCanBeTaken;
        private bool _disposed;

        public OutputChannelContext(Stream stream, AutoResetEvent contextCanBeTaken)
        {
            _stream = stream;
            _contextCanBeTaken = contextCanBeTaken;
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
            _disposed = true;
            _contextCanBeTaken.Set();
        }

        #endregion

        #region Implementation of IOutputChannelContext

        public void Write(IFrame frame)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
            
            _stream.WriteBuffer(frame.ToArray());
            _stream.Flush();
        }

        public IFrame Read()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
            
            var bytes = _stream.TakeBuffer();
            return new Frame(bytes);
        }

        #endregion
    }
}