using System.IO;
using EloquentObjects.Channels;
using EloquentObjects.Channels.Implementation;
using EloquentObjects.RPC.Messages.Session;

namespace EloquentObjects.RPC.Messages
{
    internal abstract class Message
    {
        protected Message(IHostAddress clientHostAddress)
        {
            ClientHostAddress = clientHostAddress;
        }

        public abstract MessageType MessageType { get; }
        
        public IHostAddress ClientHostAddress { get; }

        public void Write(Stream stream)
        {
            stream.WriteByte((byte)MessageType);
            ClientHostAddress.Write(stream);
            WriteInternal(stream);
            stream.Flush();
        }
        
        protected abstract void WriteInternal(Stream stream);

        public static Message Read(Stream stream)
        {
            var messageType = (MessageType)stream.TakeByte();
            var clientHostAddress = HostAddress.Read(stream);

            switch (messageType)
            {
                case MessageType.Hello:
                    return HelloMessage.ReadInternal(clientHostAddress, stream);
                case MessageType.HelloAck:
                    return HelloAckMessage.ReadInternal(clientHostAddress, stream);
                case MessageType.Heartbeat:
                    return HeartbeatMessage.ReadInternal(clientHostAddress);
                case MessageType.Disconnect:
                    return DisconnectMessage.ReadInternal(clientHostAddress, stream);
                case MessageType.TerminateSession:
                    return TerminateMessage.ReadInternal(clientHostAddress);
                case MessageType.Exception:
                    return ExceptionMessage.ReadInternal(clientHostAddress, stream);
                case MessageType.Event:
                    return EventMessage.ReadInternal(clientHostAddress, stream);
                case MessageType.Request:
                    return RequestMessage.ReadInternal(clientHostAddress, stream);
                case MessageType.Response:
                    return ResponseMessage.ReadInternal(clientHostAddress, stream);
                case MessageType.EloquentObject:
                    return EloquentObjectMessage.ReadInternal(clientHostAddress, stream);
                default:
                    throw new IOException(
                        $"Unknown Message type {messageType} is received");
            }
        }
    }
}