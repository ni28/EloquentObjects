using System;
using System.Collections.Generic;
using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Server
{
    internal interface IEndpointHub : IDisposable
    {
        bool TryConnectEndpoint(string endpointId, IHostAddress clientHostAddress, int connectionId, IOutputChannel outputChannel, out IConnection connection);
        IDisposable AddEndpoint(string endpointId, IEndpoint endpoint);
    }
}