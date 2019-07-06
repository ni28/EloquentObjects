using System.IO;
using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.Session
{
    internal sealed class HelloSessionMessage : SessionMessage
    {
        internal HelloSessionMessage(IHostAddress clientHostAddress, string endpointId, int connectionId) : base(
            clientHostAddress)
        {
            EndpointId = endpointId;
            ConnectionId = connectionId;
        }

        public string EndpointId { get; }
        public int ConnectionId { get; }

        #region Overrides of SessionMessage

        public override SessionMessageType MessageType => SessionMessageType.Hello;
        protected override void WriteInternal(Stream stream)
        {
            stream.WriteString(EndpointId);
            stream.WriteInt(ConnectionId);
            stream.Flush();
        }

        #endregion

        public static SessionMessage ReadInternal(IHostAddress hostAddress, Stream stream)
        {
            var endpointId = stream.TakeString();
            var connectionId = stream.TakeInt();
            return new HelloSessionMessage(hostAddress, endpointId, connectionId);
        }
        
        public HelloAckSessionMessage CreateAck(bool acknowledged)
        {
            return new HelloAckSessionMessage(ClientHostAddress, acknowledged);
        }
    }
}