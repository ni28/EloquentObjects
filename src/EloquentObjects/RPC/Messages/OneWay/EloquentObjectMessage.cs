using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.OneWay
{
    internal sealed class EloquentObjectMessage : OneWayMessage
    {
        internal EloquentObjectMessage(IHostAddress clientHostAddress, string objectId, byte[] payload): base(clientHostAddress)
        {
            ObjectId = objectId;
            Payload = payload;
        }

        public string ObjectId { get; }
        public byte[] Payload { get; }
        
        #region Overrides of Message

        public override MessageType MessageType => MessageType.EloquentObject;
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
            return new EloquentObjectMessage(clientHostAddress, objectId, payload);
        }
    }
}