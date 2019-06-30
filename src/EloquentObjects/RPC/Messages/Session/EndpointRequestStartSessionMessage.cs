using System.IO;
using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.Session
{
    internal sealed class EndpointRequestStartSessionMessage : SessionMessage
    {
        internal EndpointRequestStartSessionMessage(IHostAddress clientHostAddress, int connectionId) : base(clientHostAddress)
        {
            ConnectionId = connectionId;
        }

        #region Overrides of SessionMessage

        public override SessionMessageType MessageType => SessionMessageType.EndpointRequestStart;
        
        public int ConnectionId { get; }
        
        protected override void WriteInternal(Stream stream)
        {
            stream.WriteInt(ConnectionId);
            //Do not flush as the endpoint response message will be written next
        }

        #endregion

        public static SessionMessage ReadInternal(IHostAddress hostAddress, Stream stream)
        {
            var connectionId = stream.TakeInt();
            return new EndpointRequestStartSessionMessage(hostAddress, connectionId);
        }
    }
}