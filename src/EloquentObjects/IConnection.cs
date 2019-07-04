using System;

namespace EloquentObjects
{
    /// <summary>
    /// Represents a connection object that provides an access to remote object on client side. Object is disconnected when connection object is disposed.
    /// </summary>
    /// <typeparam name="T">Eloquent contract (an interface which attributed members can be accessed remotely)</typeparam>
    public interface IConnection<out T> : IDisposable
    {
        /// <summary>
        /// Gets an object proxy that allows to use a remote object as a regular object (hiding the fact that the processing is remote)
        /// </summary>
        T Object { get; }
    }
}