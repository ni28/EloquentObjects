using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.Session
{
    internal sealed class ResponseMessage : Message
    {
        internal ResponseMessage(IHostAddress clientHostAddress, byte[] payload): base(clientHostAddress)
        {
            Payload = payload;
        }
        
        public byte[] Payload { get; }
        
        #region Overrides of Message

        public override MessageType MessageType => MessageType.Response;
        protected override void WriteInternal(IFrameBuilder builder)
        {
            builder.WriteBuffer(Payload);
        }

        #endregion

        public static Message ReadInternal(IHostAddress clientHostAddress, IFrame frame)
        {
            var payload = frame.TakeBuffer();
            return new ResponseMessage(clientHostAddress, payload);
        }
    }
}