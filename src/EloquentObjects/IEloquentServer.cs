using System;
using System.Threading;
using JetBrains.Annotations;

namespace EloquentObjects
{
    /// <summary>
    /// Represents an EloquentObjects server that hosts added objects so they are available remotely.
    /// </summary>
    public interface IEloquentServer: IDisposable
    {
        /// <summary>
        /// Adds a new object to hosted objects.
        /// </summary>
        /// <param name="objectId">Identifier that can be used by a client to access this object remotely</param>
        /// <param name="obj">Object that implements the Eloquent interface</param>
        /// <param name="synchronizationContext"></param>
        /// <typeparam name="T">Eloquent contract (an interface which attributed members can be accessed remotely)</typeparam>
        void Add<T>(string objectId, T obj, [CanBeNull] SynchronizationContext synchronizationContext = null);
    }
}