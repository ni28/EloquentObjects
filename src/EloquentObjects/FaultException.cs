using System;
using System.IO;

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
        public FaultException(string message, string stackTrace, string exceptionType) : base(message)
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
        public FaultException(string message, string stackTrace, string exceptionType, FaultException innerException) : base(
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

        public void Write(Stream stream)
        {
            stream.WriteString(Message);
            stream.WriteString(StackTrace);
            stream.WriteString(ExceptionType);
            if (InnerException != null)
            {
                stream.WriteBool(true);
                _innerException.Write(stream);
            }
            else
            {
                stream.WriteBool(false);
            }
        }

        public static FaultException Read(Stream stream)
        {
            var message = stream.TakeString();
            var stackTrace = stream.TakeString();
            var exceptionType = stream.TakeString();
            var hasException = stream.TakeBool();

            FaultException innerException = null;
            if (hasException)
            {
                innerException = Read(stream);
            }

            return innerException == null
                ? new FaultException(message, stackTrace, exceptionType)
                : new FaultException(message, stackTrace, exceptionType, innerException);
        }

        public static FaultException Create(Exception exception)
        {
            return exception.InnerException == null
                ? new FaultException(exception.Message, exception.StackTrace, exception.GetType().FullName)
                : new FaultException(exception.Message, exception.StackTrace, exception.GetType().FullName,
                    Create(exception.InnerException));
        }
    }
}