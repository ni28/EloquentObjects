using System;
using System.IO;
using EloquentObjects.Channels;
using EloquentObjects.RPC.Messages;
using EloquentObjects.RPC.Messages.OneWay;

namespace EloquentObjects.RPC
{
    internal static class OutputChannelExtensions
    {
        public static void Send(this IOutputChannel outputChannel, OneWayMessage message, string side="")
        {
            outputChannel.Write(message.ToFrame(side));
        }
        
        public static void SendWithAck(this IOutputChannel outputChannel, AcknowledgedMessage message)
        {
            Message response;

            try
            {
                outputChannel.Write(message.ToFrame());
                response = Message.Create(outputChannel.Read());
            }
            catch (Exception e)
            {
                throw new IOException("Connection failed. Check that server is still alive", e);
            }

            switch (response)
            {
                case ErrorMessage errorMessage:
                    throw errorMessage.ToException();
                case ExceptionMessage exceptionMessage:
                    throw exceptionMessage.Exception;
                case AckMessage _:
                    return;
                default:
                    throw new IOException($"Unexpected session message type: {response.MessageType}");
            }
        }
    }
}