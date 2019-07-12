using System;
using System.Collections.Generic;
using System.Text;

namespace EloquentObjects.Channels.Implementation
{
    internal sealed class FrameBuilder: IFrameBuilder
    {
        private readonly List<byte> _bytes;

        public const int DefaultCapacity = 1024;

        public FrameBuilder()
        {
            _bytes = new List<byte>(DefaultCapacity);
        }

        #region Implementation of IFrameBuilder

        public void WriteByte(byte value)
        {
            _bytes.Add(value);
        }

        public void WriteBool(bool value)
        {
            _bytes.Add((byte) (value ? 1 : 0));
        }

        public void WriteInt(int value)
        {
            var bytes = BitConverter.GetBytes(value);
            WriteBytes(bytes);
        }

        public void WriteString(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(string.IsNullOrEmpty(str) ? "" : str);
            WriteInt(bytes.Length);
            WriteBytes(bytes);
        }

        public void WriteBuffer(byte[] bytes)
        {
            WriteInt(bytes.Length);
            WriteBytes(bytes);
        }

        public IFrame ToFrame()
        {
            return new Frame(_bytes.ToArray());
        }

        #endregion

        private void WriteBytes(byte[] bytes)
        {
            _bytes.AddRange(bytes);
        }

    }
}