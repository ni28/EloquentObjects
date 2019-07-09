using System.IO;
using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.Session
{
    internal sealed class RequestMessage : Message
    {
        internal RequestMessage(IHostAddress clientHostAddress, string endpointId, int connectionId, byte[] payload): base(clientHostAddress)
        {
            EndpointId = endpointId;
            ConnectionId = connectionId;
            Payload = payload;
        }

        public string EndpointId { get; }
        public int ConnectionId { get; }
        public byte[] Payload { get; }

        #region Overrides of Message

        public override MessageType MessageType => MessageType.Request;
        protected override void WriteInternal(Stream stream)
        {
            stream.WriteString(EndpointId);
            stream.WriteInt(ConnectionId);
            stream.WritePayload(Payload);
        }

        #endregion

        public static Message ReadInternal(IHostAddress clientHostAddress, Stream stream)
        {
            var endpointId = stream.TakeString();
            var connectionId = stream.TakeInt();
            var payload = stream.TakePayload();

            return new RequestMessage(clientHostAddress, endpointId, connectionId, payload);
        }
    }
}