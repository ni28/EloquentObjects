using System.IO;
using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.Session
{
    internal sealed class DisconnectSessionMessage : SessionMessage
    {
        public DisconnectSessionMessage(IHostAddress clientHostAddress, int connectionId) : base(
            clientHostAddress)
        {
            ConnectionId = connectionId;
        }

        public int ConnectionId { get; }

        #region Overrides of SessionMessage

        public override SessionMessageType MessageType => SessionMessageType.Disconnect;
        protected override void WriteInternal(Stream stream)
        {
            stream.WriteInt(ConnectionId);
            stream.Flush();
        }

        #endregion

        public static SessionMessage ReadInternal(IHostAddress hostAddress, Stream stream)
        {
            var connectionId = stream.TakeInt();
            return new DisconnectSessionMessage(hostAddress, connectionId);
        }
    }
}