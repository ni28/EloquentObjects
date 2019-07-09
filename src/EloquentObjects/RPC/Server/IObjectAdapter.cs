using System;
using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Server
{
    internal interface IObjectAdapter : IDisposable
    {
        IConnection Connect(string objectId,
            IHostAddress clientHostAddress, int connectionId,
            IOutputChannel outputChannel);
    }
}