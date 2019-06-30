using System.IO;
using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.Session
{
    internal sealed class ExceptionSessionMessage : SessionMessage
    {
        internal ExceptionSessionMessage(IHostAddress clientHostAddress, FaultException exception) : base(
            clientHostAddress)
        {
            Exception = exception;
        }

        public FaultException Exception { get; }

        #region Overrides of SessionMessage

        public override SessionMessageType MessageType => SessionMessageType.Exception;
        protected override void WriteInternal(Stream stream)
        {
            Exception.Write(stream);
            stream.Flush();
        }

        #endregion

        public static SessionMessage ReadInternal(IHostAddress clientHostAddress, Stream stream)
        {
            var exception = FaultException.Read(stream);
            return new ExceptionSessionMessage(clientHostAddress, exception);
        }
    }
}