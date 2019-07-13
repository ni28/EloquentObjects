namespace EloquentObjects.RPC.Messages
{
    internal enum ErrorType
    {
        ObjectNotFound = 0,
        EventNotFound,
        EventAlreadySubscribed
    }
}