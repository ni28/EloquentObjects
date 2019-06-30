using System.Runtime.Serialization;

namespace EloquentObjectsBenchmark.EloquentObjects
{
    [DataContract]
    public sealed class InnerParameter2
    {
        [DataMember]
        public InnerParameter3 Parameter3 { get; set; }
    }
}