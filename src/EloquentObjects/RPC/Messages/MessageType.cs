namespace EloquentObjects.RPC.Messages
{
    internal enum MessageType
    {
        Hello = 0,
        HelloAck = 1,
        Heartbeat = 2,
        Disconnect = 3,
        TerminateSession = 4,
        Exception = 5,
        Event = 6,
        Request = 7,
        Response = 8,
        EloquentObject = 9
    }
}