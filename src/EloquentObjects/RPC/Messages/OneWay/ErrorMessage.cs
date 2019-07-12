using System;
using System.Collections.Generic;
using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages.OneWay
{
    internal sealed class ErrorMessage : OneWayMessage
    {
        private ErrorType _errorType;
        private string _message;

        public ErrorMessage(IHostAddress clientHostAddress, ErrorType errorType, string message) : base(
            clientHostAddress)
        {
            _errorType = errorType;
            _message = message;
        }

        #region Overrides of Message

        public override MessageType MessageType => MessageType.ErrorMessage;

        protected override void WriteInternal(IFrameBuilder builder)
        {
            builder.WriteByte((byte) _errorType);
            builder.WriteString(_message);
        }

        #endregion

        public static Message ReadInternal(IHostAddress clientHostAddress, IFrame frame)
        {
            var errorType = (ErrorType) frame.TakeByte();
            var message = frame.TakeString();

            return new ErrorMessage(clientHostAddress, errorType, message);
        }

        public Exception ToException()
        {
            switch (_errorType)
            {
                case ErrorType.ObjectNotFound:
                    throw new KeyNotFoundException(_message);
                default:
                    throw new ArgumentOutOfRangeException(nameof(_errorType));
            }
        }
    }
}