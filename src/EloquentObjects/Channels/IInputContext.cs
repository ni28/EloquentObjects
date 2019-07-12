namespace EloquentObjects.Channels
{
    /// <summary>
    /// Represents a context object that contains a received frame and allows to write a response.
    /// </summary>
    internal interface IInputContext
    {
        /// <summary>
        /// Received frame.
        /// </summary>
        IFrame Frame { get; }
        
        /// <summary>
        /// Writes the response frame to the channel.
        /// </summary>
        void Write(IFrame frame);
    }
}