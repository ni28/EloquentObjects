using System.IO;
using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.Session
{
    internal sealed class TerminateSessionSessionMessage : SessionMessage
    {
        internal TerminateSessionSessionMessage(IHostAddress clientHostAddress) : base(clientHostAddress)
        {
        }

        #region Overrides of SessionMessage

        public override SessionMessageType MessageType => SessionMessageType.TerminateSession;
        protected override void WriteInternal(Stream stream)
        {
            stream.Flush();
        }

        #endregion

        public static SessionMessage ReadInternal(IHostAddress hostAddress)
        {
            return new TerminateSessionSessionMessage(hostAddress);
        }
    }
}