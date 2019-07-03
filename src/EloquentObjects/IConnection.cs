using System;

namespace EloquentObjects
{
    public interface IConnection<T> : IDisposable
    {
        T Object { get; }
    }
}