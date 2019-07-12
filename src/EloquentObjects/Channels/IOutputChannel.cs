using System;

namespace EloquentObjects.Channels
{
    /// <summary>
    /// Represents an output channel that can write messages and read responses.
    /// </summary>
    internal interface IOutputChannel : IDisposable
    {
        /// <summary>
        /// Writes the frame to the channel.
        /// </summary>
        void Write(IFrame frame);

        /// <summary>
        /// Read a response.
        /// </summary>
        /// <returns>A received frame object</returns>
        IFrame Read();
    }
}