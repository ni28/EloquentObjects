using System;
using System.IO;

namespace EloquentObjects.Channels.Implementation
{
    internal static class StreamExtensions
    {
        public static int TakeInt(this Stream stream)
        {
            var bytes = TakeBytes(stream, 4);
            return BitConverter.ToInt32(bytes, 0);
        }
        
        public static void WriteInt(this Stream stream, int value)
        {
            var bytes = BitConverter.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static byte[] TakeBuffer(this Stream stream)
        {
            var length = stream.TakeInt();
            var bytes = TakeBytes(stream, length);
            return bytes;
        }
                
        public static void WriteBuffer(this Stream stream, byte[] bytes)
        {
            stream.WriteInt(bytes.Length);
            stream.Write(bytes, 0, bytes.Length);
        }
        
        private static byte[] TakeBytes(Stream stream, int count)
        {
            var buffer = new byte[count];
            var bytesRead = stream.Read(buffer, 0, count);
            if (bytesRead != count)
            {
                throw new IOException($"Failed reading {count} bytes from the stream. End of the stream reached");
            }

            return buffer;
        }
    }
}