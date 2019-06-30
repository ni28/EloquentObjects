using System.IO;
using System.Threading;

namespace EloquentObjects.Channels.Implementation
{
    internal sealed class OutputChannelContext : IOutputChannelContext
    {
        private readonly AutoResetEvent _resetEvent;

        public OutputChannelContext(Stream stream, AutoResetEvent resetEvent)
        {
            _resetEvent = resetEvent;
            Stream = stream;
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            _resetEvent.Set();
        }

        #endregion

        #region Implementation of IOutputChannelContext

        public Stream Stream { get; }

        #endregion
    }
}