using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.OneWay
{
    internal sealed class EventMessage : OneWayMessage
    {
        public EventMessage(IHostAddress clientHostAddress, string objectId, string eventName, byte[] payload): base(clientHostAddress)
        {
            ObjectId = objectId;
            EventName = eventName;
            Payload = payload;
        }
        
        public string ObjectId { get; }
        public string EventName { get; }
        public byte[] Payload { get; }

        #region Overrides of Message

        public override MessageType MessageType => MessageType.Event;
        
        protected override void WriteInternal(IFrameBuilder builder)
        {
            builder.WriteString(ObjectId);
            builder.WriteString(EventName);
            builder.WriteBuffer(Payload);
        }

        #endregion

        public static Message ReadInternal(IHostAddress clientHostAddress, IFrame frame)
        {
            var objectId = frame.TakeString();
            var eventName = frame.TakeString();
            var payload = frame.TakeBuffer();

            return new EventMessage(clientHostAddress, objectId, eventName, payload);
        }
    }
}