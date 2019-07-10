namespace EloquentObjects.Channels
{
    internal interface IInputContext
    {
        IFrame Frame { get; }
        
        /// <summary>
        /// Writes the response frame to the channel.
        /// </summary>
        void Write(IFrame frame);
    }
}