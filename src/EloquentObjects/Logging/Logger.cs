namespace EloquentObjects.Logging
{
    internal static class Logger
    {
        static Logger()
        {
            #if DEBUG
            Factory = new ConsoleLoggerFactory();
            #else
            Factory = new SilentLoggerFactory();
            #endif
        }
        
        public static ILoggerFactory Factory { get; set; }
    }
}