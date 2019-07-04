namespace EloquentObjects.Serialization
{
    /// <summary>
    /// Represents an object that provides information about a call.
    /// </summary>
    public sealed class CallInfo
    {
        public CallInfo(string operationName, object[] parameters)
        {
            OperationName = operationName;
            Parameters = parameters;
        }

        /// <summary>
        /// Called operation name.
        /// </summary>
        public string OperationName { get; }
        
        /// <summary>
        /// Operation parameters.
        /// </summary>
        public object[] Parameters { get; }
    }
}