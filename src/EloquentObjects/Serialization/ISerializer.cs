using System.IO;

namespace EloquentObjects.Serialization
{
    public interface ISerializer
    {
        void WriteObject(Stream stream, object obj);
        object ReadObject(Stream stream);
        void WriteCall(Stream stream, Call call);
        Call ReadCall(Stream stream);
    }
}