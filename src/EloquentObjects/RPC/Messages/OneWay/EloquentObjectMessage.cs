using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.OneWay
{
    internal sealed class EloquentObjectMessage : OneWayMessage
    {
        internal EloquentObjectMessage(IHostAddress clientHostAddress, string objectId): base(clientHostAddress)
        {
            ObjectId = objectId;
        }

        public string ObjectId { get; }
        
        #region Overrides of Message

        public override MessageType MessageType => MessageType.EloquentObject;
        protected override void WriteInternal(IFrameBuilder builder)
        {
            builder.WriteString(ObjectId);
        }

        #endregion

        public static Message ReadInternal(IHostAddress clientHostAddress, IFrame frame)
        {
            var objectId = frame.TakeString();
            return new EloquentObjectMessage(clientHostAddress, objectId);
        }
    }
}