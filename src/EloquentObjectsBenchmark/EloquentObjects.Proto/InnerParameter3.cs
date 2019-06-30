using ProtoBuf;

namespace EloquentObjectsBenchmark.EloquentObjects.Proto
{
    [ProtoContract]
    public sealed class InnerParameter3
    {
        [ProtoMember(1)]
        public int IntValue { get; set; }
        [ProtoMember(2)]
        public bool BoolValue { get; set; }
        [ProtoMember(3)]
        public double DoubleValue { get; set; }
        [ProtoMember(4)]
        public string StringValue { get; set; }
    }
}