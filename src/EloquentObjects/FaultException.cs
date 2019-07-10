using System;
using EloquentObjects.Channels;

namespace EloquentObjects
{
    /// <summary>
    ///     This is a custom implementation of the FaultException since the Mono version used in Unity3D of the FaultException
    ///     throws a NotImplementedException when serialized.
    /// </summary>
    public sealed class FaultException : Exception
    {
        private readonly FaultException _innerException;

        /// <summary>
        ///     Creates the FaultException with provided message.
        /// </summary>
        /// <param name="message">Reason of the fault.</param>
        /// <param name="stackTrace">Stack stackTrace</param>
        /// <param name="exceptionType">Exception type as a string</param>
        private FaultException(string message, string stackTrace, string exceptionType) : base(message)
        {
            StackTrace = stackTrace;
            ExceptionType = exceptionType;
        }

        /// <summary>
        ///     Creates the FaultException with provided message.
        /// </summary>
        /// <param name="message">Reason of the fault.</param>
        /// <param name="stackTrace">Stack stackTrace</param>
        /// <param name="exceptionType">Exception type as a string</param>
        /// <param name="innerException">Inner exception</param>
        private FaultException(string message, string stackTrace, string exceptionType, FaultException innerException) : base(
            message, Create(innerException))
        {
            _innerException = innerException;
            StackTrace = stackTrace;
            ExceptionType = exceptionType;
        }

        public string ExceptionType { get; }

        #region Overrides of Exception

        public override string StackTrace { get; }

        #endregion

        internal void Write(IFrameBuilder builder)
        {
            builder.WriteString(Message);
            builder.WriteString(StackTrace);
            builder.WriteString(ExceptionType);
            if (InnerException != null)
            {
                builder.WriteBool(true);
                _innerException.Write(builder);
            }
            else
            {
                builder.WriteBool(false);
            }
        }

        internal static FaultException Read(IFrame frame)
        {
            var message = frame.TakeString();
            var stackTrace = frame.TakeString();
            var exceptionType = frame.TakeString();
            var hasException = frame.TakeBool();

            FaultException innerException = null;
            if (hasException)
            {
                innerException = Read(frame);
            }

            return innerException == null
                ? new FaultException(message, stackTrace, exceptionType)
                : new FaultException(message, stackTrace, exceptionType, innerException);
        }

        internal static FaultException Create(Exception exception)
        {
            return exception.InnerException == null
                ? new FaultException(exception.Message, exception.StackTrace, exception.GetType().FullName)
                : new FaultException(exception.Message, exception.StackTrace, exception.GetType().FullName,
                    Create(exception.InnerException));
        }
    }
}