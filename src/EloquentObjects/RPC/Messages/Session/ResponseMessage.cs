using System.IO;
using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.Session
{
    internal sealed class ResponseMessage : Message
    {
        internal ResponseMessage(IHostAddress clientHostAddress, byte[] payload): base(clientHostAddress)
        {
            Payload = payload;
        }
        
        public byte[] Payload { get; }
        
        #region Overrides of Message

        public override MessageType MessageType => MessageType.Response;
        protected override void WriteInternal(Stream stream)
        {
            stream.WritePayload(Payload);
        }

        #endregion

        public static Message ReadInternal(IHostAddress clientHostAddress, Stream stream)
        {
            var payload = stream.TakePayload();
            return new ResponseMessage(clientHostAddress, payload);
        }
    }
}