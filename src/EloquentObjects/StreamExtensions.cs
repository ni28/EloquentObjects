using System;
using System.IO;
using System.Text;

namespace EloquentObjects
{
    internal static class StreamExtensions
    {
        public static byte TakeByte(this Stream stream)
        {
            var b = stream.ReadByte();
            if (b == -1)
            {
                throw new IOException("Failed reading byte from the stream. End of the stream reached");
            }
            return (byte)b;
        }

        public static bool TakeBool(this Stream stream)
        {
            return TakeByte(stream) != 0;
        }
        
        public static void WriteBool(this Stream stream, bool value)
        {
            stream.WriteByte(value ? (byte)1 : (byte)0);
        }

        
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

        public static string TakeString(this Stream stream)
        {
            var length = stream.TakeInt();
            var bytes = TakeBytes(stream, length);
            return Encoding.UTF8.GetString(bytes, 0, length);
        }
        
        public static void WriteString(this Stream stream, string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            stream.WriteInt(bytes.Length);
            stream.Write(bytes, 0, bytes.Length);
        }
        
        public static byte[] TakePayload(this Stream stream)
        {
            var length = stream.TakeInt();
            var bytes = TakeBytes(stream, length);
            return bytes;
        }
                
        public static void WritePayload(this Stream stream, byte[] bytes)
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