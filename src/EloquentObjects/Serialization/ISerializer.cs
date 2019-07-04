using System.IO;

namespace EloquentObjects.Serialization
{
    /// <summary>
    /// Represents a serializer that can be used for serializing/deserializing data sent between EloquentServer and EloquentClient instances.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Writes a serialized object to the stream.
        /// </summary>
        /// <param name="stream">Stre</param>
        /// <param name="obj"></param>
        void WriteObject(Stream stream, object obj);
        object ReadObject(Stream stream);
        void WriteCall(Stream stream, CallInfo callInfo);
        CallInfo ReadCall(Stream stream);
    }
}