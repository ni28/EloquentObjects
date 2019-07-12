using System;

namespace EloquentObjects.Channels
{
    /// <summary>
    /// Represents an input channel that can receive frames and write responses using the <see cref="IInputContext"/> object.
    /// </summary>
    internal interface IInputChannel : IDisposable
    {
        /// <summary>
        /// Starts receiving frames.
        /// </summary>
        void Start();
        
        /// <summary>
        /// Occurs when a new frame is received.
        /// </summary>
        event EventHandler<IInputContext> FrameReceived;
    }
}