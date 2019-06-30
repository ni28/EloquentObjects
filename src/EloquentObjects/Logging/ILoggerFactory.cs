using System;

namespace EloquentObjects.Logging
{
    internal interface ILoggerFactory
    {
        ILogger Create(Type type);
    }
}