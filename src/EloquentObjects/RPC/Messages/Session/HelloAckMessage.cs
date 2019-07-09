using System.IO;
using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.Session
{
    internal sealed class HelloAckMessage : Message
    {
        public bool Acknowledged { get; }

        internal HelloAckMessage(IHostAddress clientHostAddress, bool acknowledged) : base(clientHostAddress)
        {
            Acknowledged = acknowledged;
        }

        #region Overrides of Message

        public override MessageType MessageType => MessageType.HelloAck;
        protected override void WriteInternal(Stream stream)
        {
            stream.WriteBool(Acknowledged);
        }

        #endregion

        public static Message ReadInternal(IHostAddress hostAddress, Stream stream)
        {
            var acknowledged = stream.TakeBool();

            return new HelloAckMessage(hostAddress, acknowledged);
        }
    }
}