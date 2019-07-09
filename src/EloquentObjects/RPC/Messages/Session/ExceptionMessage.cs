using System.IO;
using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.Session
{
    internal sealed class ExceptionMessage : Message
    {
        internal ExceptionMessage(IHostAddress clientHostAddress, FaultException exception) : base(
            clientHostAddress)
        {
            Exception = exception;
        }

        public FaultException Exception { get; }

        #region Overrides of SessionMessage

        public override MessageType MessageType => MessageType.Exception;
        protected override void WriteInternal(Stream stream)
        {
            Exception.Write(stream);
        }

        #endregion

        public static Message ReadInternal(IHostAddress clientHostAddress, Stream stream)
        {
            var exception = FaultException.Read(stream);
            return new ExceptionMessage(clientHostAddress, exception);
        }
    }
}