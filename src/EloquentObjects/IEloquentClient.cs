using System;

namespace EloquentObjects
{
    public interface IEloquentClient: IDisposable
    {
        Connection<T> Connect<T>(string objectId) where T : class;
    }
}