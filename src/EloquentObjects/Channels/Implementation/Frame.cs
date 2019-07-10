using System;
using System.Text;

namespace EloquentObjects.Channels.Implementation
{
    internal sealed class Frame: IFrame
    {
        private readonly byte[] _bytes;
        private int _position;

        public Frame(byte[] bytes)
        {
            _bytes = bytes;
        }

        #region Implementation of IFrame

        public byte TakeByte()
        {
            if (_position >= _bytes.Length)
                throw new InvalidOperationException("End of frame reached");
            return _bytes[_position++];
        }

        public bool TakeBool()
        {
            if (_position >= _bytes.Length)
                throw new InvalidOperationException("End of frame reached");
            return _bytes[_position++] != 0;
        }

        public int TakeInt()
        {
            var bytes = TakeBytes(4);
            return BitConverter.ToInt32(bytes, 0);
        }

        public string TakeString()
        {
            var length = TakeInt();
            var bytes = TakeBytes(length);
            return Encoding.UTF8.GetString(bytes, 0, length);
        }

        public byte[] TakeBuffer()
        {
            var length = TakeInt();
            var bytes = TakeBytes(length);
            return bytes;
        }

        public byte[] ToArray()
        {
            return _bytes;
        }

        #endregion

        private byte[] TakeBytes(int length)
        {
            if (_position + length > _bytes.Length)
                throw new InvalidOperationException("End of frame reached");

            var result = new byte[length];

            for (var i = 0; i < length; i++)
            {
                result[i] = _bytes[_position++];
            }

            return result;
        }

    }
}