using System;
using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Server
{
    internal interface IEndpoint : IDisposable
    {
        IConnection Connect(string endpointId,
            IHostAddress clientHostAddress, int connectionId,
            IOutputChannel outputChannel);
    }
}