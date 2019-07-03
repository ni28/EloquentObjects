using System.IO;

namespace EloquentObjects.RPC.Client
{
    internal sealed class EndpointMessageReadyEventArgs
    {
        public EndpointMessageReadyEventArgs(int connectionId, Stream stream)
        {
            ConnectionId = connectionId;
            Stream = stream;
        }

        public int ConnectionId { get; }

        public Stream Stream { get; }
    }
}