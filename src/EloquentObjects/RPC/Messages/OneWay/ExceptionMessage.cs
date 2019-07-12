using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.OneWay
{
    internal sealed class ExceptionMessage : OneWayMessage
    {
        internal ExceptionMessage(IHostAddress clientHostAddress, FaultException exception) : base(
            clientHostAddress)
        {
            Exception = exception;
        }

        public FaultException Exception { get; }

        #region Overrides of Message

        public override MessageType MessageType => MessageType.Exception;
        protected override void WriteInternal(IFrameBuilder builder)
        {
            Exception.Write(builder);
        }

        #endregion

        public static Message ReadInternal(IHostAddress clientHostAddress, IFrame frame)
        {
            var exception = FaultException.Read(frame);
            return new ExceptionMessage(clientHostAddress, exception);
        }
    }
}