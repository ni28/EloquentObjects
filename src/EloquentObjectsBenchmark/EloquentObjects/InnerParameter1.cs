using System.Runtime.Serialization;

namespace EloquentObjectsBenchmark.EloquentObjects
{
    [DataContract]
    public sealed class InnerParameter1
    {
        [DataMember]
        public InnerParameter2 Parameter2 { get; set; }
    }
}