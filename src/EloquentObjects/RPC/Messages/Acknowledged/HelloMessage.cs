using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.Acknowledged
{
    internal sealed class HelloMessage : AcknowledgedMessage
    {
        internal HelloMessage(IHostAddress clientHostAddress) : base(clientHostAddress)
        {
        }

        #region Overrides of Message

        public override MessageType MessageType => MessageType.Hello;
        protected override void WriteInternal(IFrameBuilder builder)
        {
        }

        #endregion

        public static Message ReadInternal(IHostAddress hostAddress, IFrame frame)
        {
            return new HelloMessage(hostAddress);
        }
    }
}