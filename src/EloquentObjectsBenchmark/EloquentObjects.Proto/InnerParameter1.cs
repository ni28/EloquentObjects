using ProtoBuf;

namespace EloquentObjectsBenchmark.EloquentObjects.Proto
{
    [ProtoContract]
    public sealed class InnerParameter1
    {
        [ProtoMember(1)]
        public InnerParameter2 Parameter2 { get; set; }
    }
}