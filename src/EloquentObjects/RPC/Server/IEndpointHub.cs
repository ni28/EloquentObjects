using System;
using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Server
{
    internal interface IEndpointHub : IDisposable
    {
        IConnection ConnectEndpoint(string endpointId, IHostAddress clientHostAddress, int connectionId,
            IOutputChannel outputChannel);
        IDisposable AddEndpoint(string endpointId, IEndpoint endpoint);
        bool ContainsEndpoint(string endpointId);
    }
}