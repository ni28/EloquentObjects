using System;

namespace EloquentObjects.RPC.Client
{
    internal interface IProxy: IDisposable
    {
        void Subscribe(IConnection connection);
    }
}