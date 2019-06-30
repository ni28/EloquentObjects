namespace EloquentObjects.Serialization
{
    public sealed class Call
    {
        public Call(string operationName, object[] parameters)
        {
            OperationName = operationName;
            Parameters = parameters;
        }

        public string OperationName { get; }
        public object[] Parameters { get; }
    }
}