using ProtoBuf;

namespace EloquentObjectsBenchmark.EloquentObjects.Proto
{
    [ProtoContract]
    public sealed class InnerParameter2
    {
        [ProtoMember(1)]
        public InnerParameter3 Parameter3 { get; set; }
    }
}