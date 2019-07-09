using System;
using System.Collections.Generic;
using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Server
{
    internal interface IObjectsRepository : IDisposable
    {
        bool TryConnectObject(string objectId, IHostAddress clientHostAddress, int connectionId, IOutputChannel outputChannel, out IConnection connection);
        IDisposable Add(string objectId, IObjectAdapter objectAdapter);
    }
}