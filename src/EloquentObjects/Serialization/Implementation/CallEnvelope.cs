using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace EloquentObjects.Serialization.Implementation
{
    [DataContract]
    internal sealed class CallEnvelope
    {
        internal CallEnvelope(
            [NotNull] string operationName,
            [NotNull] params object[] parameters)
        {
            OperationName = operationName;
            Parameters = parameters;
        }

        [NotNull] [DataMember] public string OperationName { get; private set; }
        [NotNull] [DataMember] public object[] Parameters { get; private set; }

        #region Overrides of Object

        public override string ToString()
        {
            return $"CallEnvelope: Operation {OperationName}, Passed parameters count: {Parameters.Length}";
        }

        #endregion
    }
}