using System.IO;
using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.Session
{
    internal sealed class TerminateMessage : Message
    {
        internal TerminateMessage(IHostAddress clientHostAddress) : base(clientHostAddress)
        {
        }

        #region Overrides of SessionMessage

        public override MessageType MessageType => MessageType.TerminateSession;
        protected override void WriteInternal(Stream stream)
        {
        }

        #endregion

        public static Message ReadInternal(IHostAddress hostAddress)
        {
            return new TerminateMessage(hostAddress);
        }
    }
}