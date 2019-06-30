using System.IO;
using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.Session
{
    internal sealed class EndpointResponseStartSessionMessage : SessionMessage
    {
        internal EndpointResponseStartSessionMessage(IHostAddress clientHostAddress) : base(clientHostAddress)
        {
        }

        #region Overrides of SessionMessage

        public override SessionMessageType MessageType => SessionMessageType.EndpointResponseStart;
        
        protected override void WriteInternal(Stream stream)
        {
            //Do not flush as the endpoint response message will be written next
        }

        #endregion

        public static SessionMessage ReadInternal(IHostAddress hostAddress)
        {
            return new EndpointResponseStartSessionMessage(hostAddress);
        }
    }
}