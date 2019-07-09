using System.IO;
using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.Session
{
    internal sealed class HeartbeatMessage : Message
    {
        internal HeartbeatMessage(IHostAddress clientHostAddress) : base(clientHostAddress)
        {
        }

        #region Overrides of SessionMessage

        public override MessageType MessageType => MessageType.Heartbeat;
        protected override void WriteInternal(Stream stream)
        {
        }

        #endregion

        public static Message ReadInternal(IHostAddress hostAddress)
        {
            return new HeartbeatMessage(hostAddress);
        }
    }
}