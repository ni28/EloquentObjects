using System;

namespace EloquentObjects.RPC.Client
{
    internal sealed class NotifyEventArgs: EventArgs
    {
        public NotifyEventArgs(string eventName, object[] parameters)
        {
            EventName = eventName;
            Parameters = parameters;
        }

        public string EventName { get; }
        public object[] Parameters { get; }
    }
}