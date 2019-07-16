using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.OneWay
{
    internal sealed class ResponseMessage : OneWayMessage
    {
        internal ResponseMessage(IHostAddress clientHostAddress, byte[] serializedParameters, bool[] selector, string[] objectIds): base(clientHostAddress)
        {
            SerializedParameters = serializedParameters;
            Selector = selector;
            ObjectIds = objectIds;
        }
        
        public byte[] SerializedParameters { get; }
        public bool[] Selector { get; }
        public string[] ObjectIds { get; }

        #region Overrides of Message

        public override MessageType MessageType => MessageType.Response;
        protected override void WriteInternal(IFrameBuilder builder)
        {
            builder.WriteBuffer(SerializedParameters);
            builder.WriteBoolArray(Selector);
            builder.WriteStringArray(ObjectIds);
        }

        #endregion

        public static Message ReadInternal(IHostAddress clientHostAddress, IFrame frame)
        {
            var payload = frame.TakeBuffer();
            var selector = frame.TakeBoolArray();
            var objectIds = frame.TakeStringArray();
            return new ResponseMessage(clientHostAddress, payload, selector, objectIds);
        }
    }
}