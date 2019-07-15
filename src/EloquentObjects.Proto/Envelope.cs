using ProtoBuf;

namespace EloquentObjects.Proto
{
    [ProtoContract]
    internal sealed class Envelope
    {
        internal Envelope(params object[] parameters)
        {
            Parameters = parameters;
        }

        [ProtoMember(1)] public object[] Parameters { get; private set; }

        #region Overrides of Object

        public override string ToString()
        {
            return $"CallEnvelope: Passed parameters count: {Parameters.Length}";
        }

        #endregion
    }
}