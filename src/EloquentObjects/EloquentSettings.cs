namespace EloquentObjects
{
    public sealed class EloquentSettings
    {
        public int HeartBeatMs { get; set; } = 1000;

        public int MaxHeartBeatLost { get; set; } = 3;
        
        public int SendTimeout { get; set; } = 0;

        public int ReceiveTimeout { get; set; } = 0;
    }
}