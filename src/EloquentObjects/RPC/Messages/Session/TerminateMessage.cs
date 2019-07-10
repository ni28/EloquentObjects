using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.Session
{
    internal sealed class TerminateMessage : Message
    {
        internal TerminateMessage(IHostAddress clientHostAddress) : base(clientHostAddress)
        {
        }

        #region Overrides of Message

        public override MessageType MessageType => MessageType.TerminateSession;
        protected override void WriteInternal(IFrameBuilder builder)
        {
        }

        #endregion

        public static Message ReadInternal(IHostAddress hostAddress)
        {
            return new TerminateMessage(hostAddress);
        }
    }
}