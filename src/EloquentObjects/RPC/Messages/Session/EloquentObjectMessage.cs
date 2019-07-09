using System.IO;
using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.Session
{
    internal sealed class EloquentObjectMessage : Message
    {
        internal EloquentObjectMessage(IHostAddress clientHostAddress, string objectId, byte[] payload): base(clientHostAddress)
        {
            ObjectId = objectId;
            Payload = payload;
        }

        public string ObjectId { get; }
        public byte[] Payload { get; }
        
        #region Overrides of Message

        public override MessageType MessageType => MessageType.EloquentObject;
        protected override void WriteInternal(Stream stream)
        {
            stream.WriteString(ObjectId);
            stream.WritePayload(Payload);
        }

        #endregion

        public static Message ReadInternal(IHostAddress clientHostAddress, Stream stream)
        {
            var objectId = stream.TakeString();
            var payload = stream.TakePayload();
            return new EloquentObjectMessage(clientHostAddress, objectId, payload);
        }
    }
}