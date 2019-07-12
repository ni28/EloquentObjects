using EloquentObjects.Channels;
using EloquentObjects.RPC.Messages.OneWay;

namespace EloquentObjects.RPC.Messages.Acknowledged
{
    internal sealed class ConnectMessage : AcknowledgedMessage
    {
        internal ConnectMessage(IHostAddress clientHostAddress, string objectId) : base(
            clientHostAddress)
        {
            ObjectId = objectId;
        }

        public string ObjectId { get; }
        #region Overrides of Message

        public override MessageType MessageType => MessageType.Connect;
        protected override void WriteInternal(IFrameBuilder builder)
        {
            builder.WriteString(ObjectId);
        }

        #endregion

        public static Message ReadInternal(IHostAddress hostAddress, IFrame frame)
        {
            var objectId = frame.TakeString();
            return new ConnectMessage(hostAddress, objectId);
        }
    }
}