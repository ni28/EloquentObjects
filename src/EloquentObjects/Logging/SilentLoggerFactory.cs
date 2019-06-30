using System;

namespace EloquentObjects.Logging
{
    internal sealed class SilentLoggerFactory : ILoggerFactory
    {
        #region Implementation of ILoggerFactory

        public ILogger Create(Type type)
        {
            return new SilentLogger(type);
        }

        #endregion
    }
}