using System;
using System.Threading;
using JetBrains.Annotations;

namespace EloquentObjects
{
    public interface IEloquentServer: IDisposable
    {
        void Add<T>(string objectId, T obj, [CanBeNull] SynchronizationContext synchronizationContext = null);
    }
}