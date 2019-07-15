using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace EloquentObjects.Serialization.Implementation
{
    [DataContract]
    internal sealed class Envelope
    {
        internal Envelope([NotNull] params object[] parameters)
        {
            Parameters = parameters;
        }

        [NotNull] [DataMember] public object[] Parameters { get; private set; }

        #region Overrides of Object

        public override string ToString()
        {
            return $"CallEnvelope: Passed parameters count: {Parameters.Length}";
        }

        #endregion
    }
}