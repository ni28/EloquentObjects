using System.IO;

namespace EloquentObjects.Channels.Implementation
{
    internal sealed class InputContext: IInputContext
    {
        private readonly Stream _stream;

        public InputContext(Stream stream, IFrame frame)
        {
            _stream = stream;
            Frame = frame;
        }
        
        #region Implementation of IInputContext

        public IFrame Frame { get; }
        
        public void Write(IFrame frame)
        {
            _stream.WriteBuffer(frame.ToArray());
            _stream.Flush();
        }

        #endregion
    }
}