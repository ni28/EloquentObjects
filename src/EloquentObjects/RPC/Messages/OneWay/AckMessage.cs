using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.OneWay
{
    internal sealed class AckMessage : OneWayMessage
    {
        internal AckMessage(IHostAddress clientHostAddress) : base(clientHostAddress)
        {
        }

        #region Overrides of Message

        public override MessageType MessageType => MessageType.Ack;
        protected override void WriteInternal(IFrameBuilder builder)
        {
        }

        #endregion

        public static Message ReadInternal(IHostAddress hostAddress, IFrame frame)
        {
            return new AckMessage(hostAddress);
        }
    }
}