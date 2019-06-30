using ProtoBuf;

namespace EloquentObjectsBenchmark.EloquentObjects.Proto
{
    [ProtoContract]
    public sealed class Parameter
    {
        [ProtoMember(1)]
        public InnerParameter1 Parameter1 { get; set; }
    }
}