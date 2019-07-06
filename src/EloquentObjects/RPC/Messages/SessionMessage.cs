using System.IO;
using EloquentObjects.Channels;
using EloquentObjects.Channels.Implementation;
using EloquentObjects.RPC.Messages.Session;

namespace EloquentObjects.RPC.Messages
{
    internal abstract class SessionMessage
    {
        protected SessionMessage(IHostAddress clientHostAddress)
        {
            ClientHostAddress = clientHostAddress;
        }

        public abstract SessionMessageType MessageType { get; }
        
        public IHostAddress ClientHostAddress { get; }

        public void Write(Stream stream)
        {
            stream.WriteByte((byte)MessageType);
            ClientHostAddress.Write(stream);
            WriteInternal(stream);
        }
        
        protected abstract void WriteInternal(Stream stream);

        public static SessionMessage Read(Stream stream)
        {
            var messageType = (SessionMessageType)stream.TakeByte();
            var clientHostAddress = HostAddress.Read(stream);

            switch (messageType)
            {
                case SessionMessageType.Hello:
                    return HelloSessionMessage.ReadInternal(clientHostAddress, stream);
                case SessionMessageType.HelloAck:
                    return HelloAckSessionMessage.ReadInternal(clientHostAddress, stream);
                case SessionMessageType.Heartbeat:
                    return HeartbeatSessionMessage.ReadInternal(clientHostAddress);
                case SessionMessageType.EndpointRequestStart:
                    return EndpointRequestStartSessionMessage.ReadInternal(clientHostAddress, stream);
                case SessionMessageType.EndpointResponseStart:
                    return EndpointResponseStartSessionMessage.ReadInternal(clientHostAddress);
                case SessionMessageType.Disconnect:
                    return DisconnectSessionMessage.ReadInternal(clientHostAddress, stream);
                case SessionMessageType.TerminateSession:
                    return TerminateSessionSessionMessage.ReadInternal(clientHostAddress);
                case SessionMessageType.Exception:
                    return ExceptionSessionMessage.ReadInternal(clientHostAddress, stream);
                default:
                    throw new IOException("Unknown Session Message type received");
            }
        }
    }
}