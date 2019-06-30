using System;

namespace EloquentObjects.Logging
{
    internal sealed class SilentLogger : ILogger
    {
        public SilentLogger(Type type)
        {
        }

        #region Implementation of ILogger

        public void Debug(Func<string> messageFactory)
        {
        }

        public void Info(Func<string> messageFactory)
        {
        }

        public void Warning(Func<string> messageFactory)
        {
        }

        public void Error(Func<string> messageFactory)
        {
        }

        public void Exception(Func<string> messageFactory, Exception exception)
        {
        }

        #endregion
    }
}