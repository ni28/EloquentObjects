using System.IO;
using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.Session
{
    internal sealed class EventMessage : Message
    {
        public EventMessage(IHostAddress clientHostAddress, string endpointId, int connectionId, byte[] payload): base(clientHostAddress)
        {
            EndpointId = endpointId;
            ConnectionId = connectionId;
            Payload = payload;
        }
        
        //TODO: Rename to objectId?
        public string EndpointId { get; }
        public int ConnectionId { get; }
        public byte[] Payload { get; }

        #region Overrides of Message

        public override MessageType MessageType => MessageType.Event;
        
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

            return new EventMessage(clientHostAddress, endpointId, connectionId, payload);
        }
    }
}