using System.IO;
using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.Session
{
    internal sealed class DisconnectMessage : Message
    {
        public DisconnectMessage(IHostAddress clientHostAddress, int connectionId) : base(
            clientHostAddress)
        {
            ConnectionId = connectionId;
        }

        public int ConnectionId { get; }

        #region Overrides of Message

        public override MessageType MessageType => MessageType.Disconnect;
        protected override void WriteInternal(Stream stream)
        {
            stream.WriteInt(ConnectionId);
        }

        #endregion

        public static Message ReadInternal(IHostAddress hostAddress, Stream stream)
        {
            var connectionId = stream.TakeInt();
            return new DisconnectMessage(hostAddress, connectionId);
        }
    }
}