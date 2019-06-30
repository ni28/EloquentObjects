using System.IO;
using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.Session
{
    internal sealed class HeartbeatSessionMessage : SessionMessage
    {
        internal HeartbeatSessionMessage(IHostAddress clientHostAddress) : base(clientHostAddress)
        {
        }

        #region Overrides of SessionMessage

        public override SessionMessageType MessageType => SessionMessageType.Heartbeat;
        protected override void WriteInternal(Stream stream)
        {
            stream.Flush();
        }

        #endregion

        public static SessionMessage ReadInternal(IHostAddress hostAddress)
        {
            return new HeartbeatSessionMessage(hostAddress);
        }
    }
}