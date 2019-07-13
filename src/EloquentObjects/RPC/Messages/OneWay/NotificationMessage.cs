using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.OneWay
{
    internal sealed class NotificationMessage : OneWayMessage
    {
        public NotificationMessage(IHostAddress clientHostAddress, string objectId, byte[] payload): base(clientHostAddress)
        {
            ObjectId = objectId;
            Payload = payload;
        }
        
        public string ObjectId { get; }
        public byte[] Payload { get; }

        #region Overrides of Message

        public override MessageType MessageType => MessageType.Notification;
        
        protected override void WriteInternal(IFrameBuilder builder)
        {
            builder.WriteString(ObjectId);
            builder.WriteBuffer(Payload);
        }

        #endregion

        public static Message ReadInternal(IHostAddress clientHostAddress, IFrame frame)
        {
            var objectId = frame.TakeString();
            var payload = frame.TakeBuffer();

            return new NotificationMessage(clientHostAddress, objectId, payload);
        }
    }
}