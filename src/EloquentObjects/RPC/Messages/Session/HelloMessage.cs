using System.IO;
using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.Session
{
    internal sealed class HelloMessage : Message
    {
        internal HelloMessage(IHostAddress clientHostAddress, string objectId, int connectionId) : base(
            clientHostAddress)
        {
            ObjectId = objectId;
            ConnectionId = connectionId;
        }

        public string ObjectId { get; }
        public int ConnectionId { get; }

        #region Overrides of Message

        public override MessageType MessageType => MessageType.Hello;
        protected override void WriteInternal(Stream stream)
        {
            stream.WriteString(ObjectId);
            stream.WriteInt(ConnectionId);
        }

        #endregion

        public static Message ReadInternal(IHostAddress hostAddress, Stream stream)
        {
            var objectId = stream.TakeString();
            var connectionId = stream.TakeInt();
            return new HelloMessage(hostAddress, objectId, connectionId);
        }
        
        public HelloAckMessage CreateAck(bool acknowledged)
        {
            return new HelloAckMessage(ClientHostAddress, acknowledged);
        }
    }
}