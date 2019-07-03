using System;
using EloquentObjects.RPC.Client.Implementation;

namespace EloquentObjects
{
    public interface IEloquentClient: IDisposable
    {
        IConnection<T> Connect<T>(string objectId) where T : class;
    }
}