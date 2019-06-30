using System;

namespace EloquentObjects.Logging
{
    internal interface ILogger
    {
        void Debug(Func<string> messageFactory);
        void Info(Func<string> messageFactory);
        void Warning(Func<string> messageFactory);
        void Error(Func<string> messageFactory);
        void Exception(Func<string> messageFactory, Exception exception);
    }
}