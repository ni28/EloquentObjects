using System.IO;
using System.Text;
using EloquentObjects.RPC.Messages.Endpoint;
using EloquentObjects.Serialization;
using JetBrains.Annotations;

namespace EloquentObjects.RPC.Messages
{
    internal abstract class EndpointMessage
    {
        public abstract EndpointMessageType MessageType { get; }

        public void Write(Stream stream, ISerializer serializer)
        {
            stream.WriteByte((byte) MessageType);
            WriteInternal(stream, serializer);
            stream.Flush();
        }

        protected abstract void WriteInternal(Stream stream, ISerializer serializer);

        public static EndpointMessage Read(Stream stream, ISerializer serializer)
        {
            var messageType = (EndpointMessageType) stream.TakeByte();

            switch (messageType)
            {
                case EndpointMessageType.Event:
                    return EventEndpointMessage.ReadInternal(stream, serializer);
                case EndpointMessageType.Request:
                    return RequestEndpointMessage.ReadInternal(stream, serializer);
                case EndpointMessageType.Response:
                    return ResponseEndpointMessage.ReadInternal(stream, serializer);
                default:
                    throw new IOException(
                        $"Unknown Message type {messageType} is received while Endpoint message was expected");
            }
        }
        
        [NotNull]
        protected static byte[] Serialize(ISerializer serializer, [NotNull] object obj)
        {
            using (var memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, obj);
                //memoryStream.Position = 0;
                return memoryStream.ToArray();
            }
        }

        protected static object Deserialize(ISerializer deserializer, [NotNull] byte[] bytes)
        {
            using (Stream stream = new MemoryStream())
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Position = 0;
                return deserializer.ReadObject(stream);
            }
        }

        [NotNull]
        protected static byte[] SerializeCall(ISerializer serializer, CallInfo callInfo)
        {
            using (var memoryStream = new MemoryStream())
            {
                serializer.WriteCall(memoryStream, callInfo);
                //memoryStream.Position = 0;
                return memoryStream.ToArray();
            }
        }
        
        protected static CallInfo DeserializeCall(ISerializer deserializer, [NotNull] byte[] bytes)
        {
            using (Stream stream = new MemoryStream())
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Position = 0;
                return deserializer.ReadCall(stream);
            }
        }
    }
}