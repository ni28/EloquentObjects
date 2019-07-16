using System.IO;
using EloquentObjects.Channels;
using EloquentObjects.Channels.Implementation;
using EloquentObjects.RPC.Messages.Acknowledged;
using EloquentObjects.RPC.Messages.OneWay;

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

        public IFrame ToFrame()
        {
            var builder = new FrameBuilder();
            builder.WriteByte((byte)MessageType);
            builder.WriteString(ClientHostAddress.IpAddress);
            builder.WriteInt(ClientHostAddress.Port);
            WriteInternal(builder);
            
            return builder.ToFrame();
        }
        
        protected abstract void WriteInternal(IFrameBuilder frameBuilder);
        
        public static Message Create(IFrame frame, string side="")
        {
            var messageType = (MessageType)frame.TakeByte();
            var clientHostAddress = HostAddress.Read(frame);
            
            switch (messageType)
            {
                case MessageType.Hello:
                    return HelloMessage.ReadInternal(clientHostAddress, frame);
                case MessageType.Ack:
                    return AckMessage.ReadInternal(clientHostAddress, frame);
                case MessageType.Connect:
                    return ConnectMessage.ReadInternal(clientHostAddress, frame);
                case MessageType.Heartbeat:
                    return HeartbeatMessage.ReadInternal(clientHostAddress);
                case MessageType.TerminateSession:
                    return TerminateMessage.ReadInternal(clientHostAddress);
                case MessageType.ErrorMessage:
                    return ErrorMessage.ReadInternal(clientHostAddress, frame);
                case MessageType.Exception:
                    return ExceptionMessage.ReadInternal(clientHostAddress, frame);
                case MessageType.Event:
                    return EventMessage.ReadInternal(clientHostAddress, frame);
                case MessageType.Request:
                    return RequestMessage.ReadInternal(clientHostAddress, frame);
                case MessageType.Response:
                    return ResponseMessage.ReadInternal(clientHostAddress, frame);
                case MessageType.SubscribeEvent:
                    return SubscribeEventMessage.ReadInternal(clientHostAddress, frame);
                case MessageType.UnsubscribeEvent:
                    return UnsubscribeEventMessage.ReadInternal(clientHostAddress, frame);
                case MessageType.Notification:
                    return NotificationMessage.ReadInternal(clientHostAddress, frame);
                default:
                    throw new IOException(
                        $"Unknown Message type {messageType} is received");
            }
        }
    }
}