using System.IO;

namespace EloquentObjects.Serialization
{
    /// <summary>
    /// Represents a serializer that can be used for serializing/deserializing data sent between EloquentServer and EloquentClient instances.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Writes a serialized objects array to the stream.
        /// </summary>
        /// <param name="stream">Stream that will accept serialized objects</param>
        /// <param name="objects">Array that needs to be serialized</param>
        void WriteObjects(Stream stream, object[] objects);
        object[] ReadObjects(Stream stream);
    }
}