using System.IO;
using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.Session
{
    internal sealed class RequestMessage : Message
    {
        internal RequestMessage(IHostAddress clientHostAddress, string objectId, int connectionId, byte[] payload): base(clientHostAddress)
        {
            ObjectId = objectId;
            ConnectionId = connectionId;
            Payload = payload;
        }

        public string ObjectId { get; }
        public int ConnectionId { get; }
        public byte[] Payload { get; }

        #region Overrides of Message

        public override MessageType MessageType => MessageType.Request;
        protected override void WriteInternal(Stream stream)
        {
            stream.WriteString(ObjectId);
            stream.WriteInt(ConnectionId);
            stream.WritePayload(Payload);
        }

        #endregion

        public static Message ReadInternal(IHostAddress clientHostAddress, Stream stream)
        {
            var objectId = stream.TakeString();
            var connectionId = stream.TakeInt();
            var payload = stream.TakePayload();

            return new RequestMessage(clientHostAddress, objectId, connectionId, payload);
        }
    }
}