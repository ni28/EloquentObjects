using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.Session
{
    internal sealed class RequestMessage : Message
    {
        internal RequestMessage(IHostAddress clientHostAddress, string objectId, int connectionId, byte[] payload): base(clientHostAddress)
        {
            ObjectId = objectId;
            ConnectionId = connectionId;
            Payload = payload;
        }

        public string ObjectId { get; }
        public int ConnectionId { get; }
        public byte[] Payload { get; }

        #region Overrides of Message

        public override MessageType MessageType => MessageType.Request;
        protected override void WriteInternal(IFrameBuilder builder)
        {
            builder.WriteString(ObjectId);
            builder.WriteInt(ConnectionId);
            builder.WriteBuffer(Payload);
        }

        #endregion

        public static Message ReadInternal(IHostAddress clientHostAddress, IFrame frame)
        {
            var objectId = frame.TakeString();
            var connectionId = frame.TakeInt();
            var payload = frame.TakeBuffer();

            return new RequestMessage(clientHostAddress, objectId, connectionId, payload);
        }
    }
}