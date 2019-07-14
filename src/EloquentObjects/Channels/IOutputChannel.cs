using System;

namespace EloquentObjects.Channels
{
    /// <summary>
    /// Represents an output channel that can write messages and read responses.
    /// </summary>
    internal interface IOutputChannel : IDisposable
    {
        /// <summary>
        /// Starts a new atomic write/read operation.
        /// </summary>
        /// <returns>Returns a context object that can perform multiple read/write operations as a single transaction (all other calls to BeginWrite are blocked until the active context is disposed)</returns>
        IOutputChannelContext BeginReadWrite();
    }
}