using System;

namespace EloquentObjects.Logging
{
    internal sealed class ConsoleLogger: ILogger
    {
        private readonly Type _type;

        public ConsoleLogger(Type type)
        {
            _type = type;
        }

        private void Write(string message, ConsoleColor color)
        {
            var prevColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine($"[{DateTime.Now:hh:mm:ss.fff}] [{_type.Name}]: {message}");
            Console.ForegroundColor = prevColor;
        }
        
        #region Implementation of ILogger

        public void Debug(Func<string> messageFactory)
        {
            Write(messageFactory(), ConsoleColor.Gray);
        }

        public void Info(Func<string> messageFactory)
        {
            Write(messageFactory(), ConsoleColor.White);
        }

        public void Warning(Func<string> messageFactory)
        {
            Write(messageFactory(), ConsoleColor.Yellow);
        }

        public void Error(Func<string> messageFactory)
        {
            Write(messageFactory(), ConsoleColor.Red);
        }

        public void Exception(Func<string> messageFactory, Exception exception)
        {
            Write(messageFactory(), ConsoleColor.Red);
            Write(exception.ToString(), ConsoleColor.Red);
        }

        #endregion
    }
}