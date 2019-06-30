using System;

namespace EloquentObjects.Logging
{
    internal sealed class ConsoleLoggerFactory : ILoggerFactory
    {
        #region Implementation of ILoggerFactory

        public ILogger Create(Type type)
        {
            return new ConsoleLogger(type);
        }

        #endregion
    }
}