using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace EloquentObjects.Serialization
{
    internal static class SerializationExtensions
    {
        [NotNull]
        public static byte[] Serialize(this ISerializer serializer, params object[] objects)
        {
            using (var memoryStream = new MemoryStream())
            {
                serializer.WriteObjects(memoryStream, objects);
                return memoryStream.ToArray();
            }
        }

        public static object[] Deserialize(this ISerializer deserializer, [NotNull] byte[] bytes)
        {
            using (Stream stream = new MemoryStream())
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Position = 0;
                return deserializer.ReadObjects(stream);
            }
        }
    }
}