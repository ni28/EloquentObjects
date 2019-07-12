using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.Acknowledged
{
    internal sealed class SubscribeEventMessage : AcknowledgedMessage
    {
        public SubscribeEventMessage(IHostAddress clientHostAddress, string objectId, string eventName): base(clientHostAddress)
        {
            ObjectId = objectId;
            EventName = eventName;
        }
        
        public string ObjectId { get; }
        public string EventName { get; }

        #region Overrides of Message

        public override MessageType MessageType => MessageType.SubscribeEvent;
        
        protected override void WriteInternal(IFrameBuilder builder)
        {
            builder.WriteString(ObjectId);
            builder.WriteString(EventName);
        }

        #endregion

        public static Message ReadInternal(IHostAddress clientHostAddress, IFrame frame)
        {
            var objectId = frame.TakeString();
            var eventName = frame.TakeString();

            return new SubscribeEventMessage(clientHostAddress, objectId, eventName);
        }
    }
}