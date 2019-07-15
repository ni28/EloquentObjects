using System.IO;
using EloquentObjects.Serialization;
using ProtoBuf;

namespace EloquentObjects.Proto
{
    internal sealed class ProtoSerializer : ISerializer
    {
        #region Implementation of ISerializer

        public void WriteObjects(Stream stream, object[] objects)
        {
            Serializer.Serialize(stream, new Envelope(objects));
        }

        public object[] ReadObjects(Stream stream)
        {
            return Serializer.Deserialize<Envelope>(stream).Parameters;
        }

        #endregion
    }
}