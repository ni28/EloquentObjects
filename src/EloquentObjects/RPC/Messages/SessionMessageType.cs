namespace EloquentObjects.RPC.Messages
{
    internal enum SessionMessageType
    {
        Hello = 0,
        HelloAck = 1,
        Heartbeat = 2,
        EndpointRequestStart = 3,
        EndpointResponseStart = 4,
        Disconnect = 5,
        TerminateSession = 6,
        Exception = 7
    }
}