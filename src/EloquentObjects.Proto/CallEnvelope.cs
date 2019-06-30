using ProtoBuf;

namespace EloquentObjects.Proto
{
    [ProtoContract]
    internal sealed class CallEnvelope
    {
        internal CallEnvelope(
            string operationName,
            params object[] parameters)
        {
            OperationName = operationName;
            Parameters = parameters;
        }

        [ProtoMember(1)] public string OperationName { get; private set; }
        [ProtoMember(2)] public object[] Parameters { get; private set; }

        #region Overrides of Object

        public override string ToString()
        {
            return $"CallEnvelope: Operation {OperationName}, Passed parameters count: {Parameters.Length}";
        }

        #endregion
    }
}