using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.Session
{
    internal sealed class HelloMessage : Message
    {
        internal HelloMessage(IHostAddress clientHostAddress, string objectId, int connectionId) : base(
            clientHostAddress)
        {
            ObjectId = objectId;
            ConnectionId = connectionId;
        }

        public string ObjectId { get; }
        public int ConnectionId { get; }

        #region Overrides of Message

        public override MessageType MessageType => MessageType.Hello;
        protected override void WriteInternal(IFrameBuilder builder)
        {
            builder.WriteString(ObjectId);
            builder.WriteInt(ConnectionId);
        }

        #endregion

        public static Message ReadInternal(IHostAddress hostAddress, IFrame frame)
        {
            var objectId = frame.TakeString();
            var connectionId = frame.TakeInt();
            return new HelloMessage(hostAddress, objectId, connectionId);
        }
        
        public HelloAckMessage CreateAck(bool acknowledged)
        {
            return new HelloAckMessage(ClientHostAddress, acknowledged);
        }
    }
}