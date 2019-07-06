using System.IO;
using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.Session
{
    internal sealed class HelloAckSessionMessage : SessionMessage
    {
        public bool Acknowledged { get; }

        internal HelloAckSessionMessage(IHostAddress clientHostAddress, bool acknowledged) : base(clientHostAddress)
        {
            Acknowledged = acknowledged;
        }

        #region Overrides of SessionMessage

        public override SessionMessageType MessageType => SessionMessageType.HelloAck;
        protected override void WriteInternal(Stream stream)
        {
            stream.WriteBool(Acknowledged);
            stream.Flush();
        }

        #endregion

        public static SessionMessage ReadInternal(IHostAddress hostAddress, Stream stream)
        {
            var acknowledged = stream.TakeBool();

            return new HelloAckSessionMessage(hostAddress, acknowledged);
        }
    }
}