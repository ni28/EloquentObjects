using System;

namespace EloquentObjects.RPC.Client
{
    internal sealed class CallEventArgs: EventArgs
    {
        public CallEventArgs(string methodName, object[] parameters)
        {
            MethodName = methodName;
            Parameters = parameters;
        }

        public string MethodName { get; }
        public object[] Parameters { get; }
        
        public object ReturnValue { get; set; }
    }
}