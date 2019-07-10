using System;

namespace EloquentObjects.Channels
{
    internal interface IOutputChannel : IDisposable
    {
        /// <summary>
        /// Writes the frame to the channel.
        /// </summary>
        void Write(IFrame frame);

        IFrame Read();
    }
}