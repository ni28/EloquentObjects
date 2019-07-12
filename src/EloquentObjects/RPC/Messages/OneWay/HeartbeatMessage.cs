using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.OneWay
{
    internal sealed class HeartbeatMessage : OneWayMessage
    {
        internal HeartbeatMessage(IHostAddress clientHostAddress) : base(clientHostAddress)
        {
        }

        #region Overrides of Message

        public override MessageType MessageType => MessageType.Heartbeat;
        protected override void WriteInternal(IFrameBuilder builder)
        {
        }

        #endregion

        public static Message ReadInternal(IHostAddress hostAddress)
        {
            return new HeartbeatMessage(hostAddress);
        }
    }
}