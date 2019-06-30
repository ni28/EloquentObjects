using System.Runtime.Serialization;

namespace EloquentObjectsBenchmark.EloquentObjects
{
    [DataContract]
    public sealed class InnerParameter3
    {
        [DataMember]
        public int IntValue { get; set; }
        [DataMember]
        public bool BoolValue { get; set; }
        [DataMember]
        public double DoubleValue { get; set; }
        [DataMember]
        public string StringValue { get; set; }
    }
}