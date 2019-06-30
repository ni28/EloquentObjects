namespace EloquentObjects.RPC.Client
{
    internal interface ICallback
    {
        void HandleEvent(string eventName, object[] arguments);
    }
}