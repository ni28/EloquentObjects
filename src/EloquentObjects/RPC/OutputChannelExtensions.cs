using System;
using System.IO;
using EloquentObjects.Channels;
using EloquentObjects.RPC.Messages;
using EloquentObjects.RPC.Messages.OneWay;

namespace EloquentObjects.RPC
{
    //TODO: Move to channels?
    internal static class OutputChannelExtensions
    {
        public static void SendOneWay(this IOutputChannel outputChannel, OneWayMessage message)
        {
            using (var context = outputChannel.BeginReadWrite())
            {
                context.Write(message.ToFrame());
            }
        }

        public static Message SendAndReceive(this IOutputChannel outputChannel, OneWayMessage message)
        {
            try
            {
                IFrame response;
                using (var context = outputChannel.BeginReadWrite())
                {
                    context.Write(message.ToFrame());
                    response = context.Read();
                }

                return Message.Create(response);
            }
            catch (Exception e)
            {
                throw new IOException("Connection failed. Check that server is still alive", e);
            }
        }

        public static void SendWithAck(this IOutputChannel outputChannel, AcknowledgedMessage message)
        {
            IFrame response;
            try
            {
                using (var context = outputChannel.BeginReadWrite())
                {
                    context.Write(message.ToFrame());
                    response = context.Read();
                }
            }
            catch (Exception e)
            {
                throw new IOException("Connection failed. Check that server is still alive", e);
            }
            
            var responseMessage = Message.Create(response);
            
            switch (responseMessage)
            {
                case ErrorMessage errorMessage:
                    throw errorMessage.ToException();
                case ExceptionMessage exceptionMessage:
                    throw exceptionMessage.Exception;
                case AckMessage _:
                    return;
                default:
                    throw new IOException($"Unexpected message type: {responseMessage.MessageType}");
            }
        }
    }
}