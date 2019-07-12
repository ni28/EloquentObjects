namespace EloquentObjects.RPC.Messages
{
    internal enum MessageType
    {
        Hello = 0,
        Ack = 1,
        Heartbeat = 2,
        TerminateSession = 4,
        Exception = 5,
        Event = 6,
        Request = 7,
        Response = 8,
        EloquentObject = 9,
        SubscribeEvent = 10,
        UnsubscribeEvent = 11,
        Connect = 12,
        Notification = 13,
        ErrorMessage = 14
    }
}