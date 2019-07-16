using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.OneWay
{
    internal sealed class EventMessage : OneWayMessage
    {
        public EventMessage(IHostAddress clientHostAddress, string objectId, string eventName, byte[] serializedParameters, bool[] selector, string[] objectIds): base(clientHostAddress)
        {
            ObjectId = objectId;
            EventName = eventName;
            SerializedParameters = serializedParameters;
            Selector = selector;
            ObjectIds = objectIds;
        }
        
        public string ObjectId { get; }
        public string EventName { get; }
        public byte[] SerializedParameters { get; }
        public bool[] Selector { get; }
        public string[] ObjectIds { get; }

        #region Overrides of Message

        public override MessageType MessageType => MessageType.Event;
        
        protected override void WriteInternal(IFrameBuilder builder)
        {
            builder.WriteString(ObjectId);
            builder.WriteString(EventName);
            builder.WriteBuffer(SerializedParameters);
            builder.WriteBoolArray(Selector);
            builder.WriteStringArray(ObjectIds);
        }

        #endregion

        public static Message ReadInternal(IHostAddress clientHostAddress, IFrame frame)
        {
            var objectId = frame.TakeString();
            var eventName = frame.TakeString();
            var payload = frame.TakeBuffer();
            var selector = frame.TakeBoolArray();
            var objectsIds = frame.TakeStringArray();

            return new EventMessage(clientHostAddress, objectId, eventName, payload, selector, objectsIds);
        }
    }
}