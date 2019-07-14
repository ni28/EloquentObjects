using System;

namespace EloquentObjects.Channels
{
    /// <summary>
    /// Represents a communication context for an atomic operation.
    /// </summary>
    internal interface IOutputChannelContext: IDisposable
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