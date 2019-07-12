using JetBrains.Annotations;

namespace EloquentObjects.Channels
{
    internal interface IFrameBuilder
    {
        void WriteByte(byte value);
        void WriteBool(bool value);
        void WriteInt(int value);
        void WriteString(string str);
        void WriteBuffer(byte[] bytes);

        IFrame ToFrame();
    }
}