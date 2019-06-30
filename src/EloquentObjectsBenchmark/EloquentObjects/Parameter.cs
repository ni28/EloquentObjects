using System.Runtime.Serialization;

namespace EloquentObjectsBenchmark.EloquentObjects
{
    [DataContract]
    public sealed class Parameter
    {
        [DataMember]
        public InnerParameter1 Parameter1 { get; set; }
    }
}