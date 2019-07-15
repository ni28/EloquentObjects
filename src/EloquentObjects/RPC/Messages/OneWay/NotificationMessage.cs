using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.OneWay
{
    internal sealed class NotificationMessage : OneWayMessage
    {
        public NotificationMessage(IHostAddress clientHostAddress, string objectId, string methodName, byte[] payload,
            bool[] selector, string[] objectIds): base(clientHostAddress)
        {
            ObjectId = objectId;
            MethodName = methodName;
            Payload = payload;
            Selector = selector;
            ObjectIds = objectIds;
        }
        
        public string ObjectId { get; }
        public string MethodName { get; }
        public byte[] Payload { get; }
        public bool[] Selector { get; }
        public string[] ObjectIds { get; }

        #region Overrides of Message

        public override MessageType MessageType => MessageType.Notification;
        
        protected override void WriteInternal(IFrameBuilder builder)
        {
            builder.WriteString(ObjectId);
            builder.WriteString(MethodName);
            builder.WriteBuffer(Payload);
            builder.WriteBoolArray(Selector);
            builder.WriteStringArray(ObjectIds);
        }

        #endregion

        public static Message ReadInternal(IHostAddress clientHostAddress, IFrame frame)
        {
            var objectId = frame.TakeString();
            var methodName = frame.TakeString();
            var payload = frame.TakeBuffer();
            var selector = frame.TakeBoolArray();
            var objectIds = frame.TakeStringArray();

            return new NotificationMessage(clientHostAddress, objectId, methodName, payload, selector, objectIds);
        }
    }
}