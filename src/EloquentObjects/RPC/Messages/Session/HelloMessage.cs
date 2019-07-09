using System.IO;
using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.Session
{
    internal sealed class HelloMessage : Message
    {
        internal HelloMessage(IHostAddress clientHostAddress, string endpointId, int connectionId) : base(
            clientHostAddress)
        {
            EndpointId = endpointId;
            ConnectionId = connectionId;
        }

        public string EndpointId { get; }
        public int ConnectionId { get; }

        #region Overrides of Message

        public override MessageType MessageType => MessageType.Hello;
        protected override void WriteInternal(Stream stream)
        {
            stream.WriteString(EndpointId);
            stream.WriteInt(ConnectionId);
        }

        #endregion

        public static Message ReadInternal(IHostAddress hostAddress, Stream stream)
        {
            var endpointId = stream.TakeString();
            var connectionId = stream.TakeInt();
            return new HelloMessage(hostAddress, endpointId, connectionId);
        }
        
        public HelloAckMessage CreateAck(bool acknowledged)
        {
            return new HelloAckMessage(ClientHostAddress, acknowledged);
        }
    }
}