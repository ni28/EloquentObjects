namespace EloquentObjects.Channels
{
    internal interface IFrame
    {
        byte TakeByte();
        bool TakeBool();
        int TakeInt();
        string TakeString();
        byte[] TakeBuffer();
        
        byte[] ToArray();
    }
}