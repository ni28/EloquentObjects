using System.IO;
using JetBrains.Annotations;

namespace EloquentObjects.Serialization
{
    internal static class SerializationExtensions
    {
        [NotNull]
        public static byte[] Serialize(this ISerializer serializer, object obj)
        {
            using (var memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, obj);
                return memoryStream.ToArray();
            }
        }

        public static T Deserialize<T>(this ISerializer deserializer, [NotNull] byte[] bytes)
        {
            using (Stream stream = new MemoryStream())
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Position = 0;
                return (T)deserializer.ReadObject(stream);
            }
        }
        
        [NotNull]
        public static byte[] SerializeCall(this ISerializer serializer, CallInfo callInfo)
        {
            using (var memoryStream = new MemoryStream())
            {
                serializer.WriteCall(memoryStream, callInfo);
                return memoryStream.ToArray();
            }
        }
        
        public static CallInfo DeserializeCall(this ISerializer deserializer, [NotNull] byte[] bytes)
        {
            using (Stream stream = new MemoryStream())
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Position = 0;
                return (CallInfo)deserializer.ReadCall(stream);
            }
        }
    }
}