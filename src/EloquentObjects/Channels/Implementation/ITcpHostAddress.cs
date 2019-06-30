using System.Net;

namespace EloquentObjects.Channels.Implementation
{
    internal interface ITcpHostAddress : IHostAddress
    {
        IPAddress IpAddress { get; }
        int Port { get; }
    }
}