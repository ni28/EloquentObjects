using System;

namespace EloquentObjects.Channels
{
    internal interface IOutputChannel : IDisposable
    {
        /// <summary>
        /// Starts writing context.
        /// </summary>
        IOutputChannelContext BeginWriteRead();
    }
}