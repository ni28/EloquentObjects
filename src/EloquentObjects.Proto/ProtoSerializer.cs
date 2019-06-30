using System.IO;
using EloquentObjects.Serialization;
using ProtoBuf;

namespace EloquentObjects.Proto
{
    internal sealed class ProtoSerializer : ISerializer
    {
        #region Implementation of ISerializer

        public void WriteObject(Stream stream, object obj)
        {
            Serializer.Serialize(stream, obj);
        }

        public object ReadObject(Stream stream)
        {
            return Serializer.Deserialize<object>(stream);
        }

        public void WriteCall(Stream stream, Call call)
        {
            Serializer.Serialize(stream, new CallEnvelope(call.OperationName, call.Parameters));
        }

        public Call ReadCall(Stream stream)
        {
            var envelope = Serializer.Deserialize<CallEnvelope>(stream);
            return new Call(envelope.OperationName, envelope.Parameters);
        }

        #endregion
    }
}