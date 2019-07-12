using System;
using JetBrains.Annotations;

namespace EloquentObjects
{
    /// <summary>
    /// Represents an information about Eloquent object.
    /// </summary>
    public interface IEloquent
    {
        /// <summary>
        /// Object identifier for which a connection will be created.
        /// </summary>
        [NotNull]
        string ObjectId { get; }
        
        /// <summary>
        /// Additional information about object (if provided). This can be either a simple or a serializable object (with attributes that match current serializer).
        /// </summary>
        [CanBeNull]
        object Info { get; }
    }
    
    /// <summary>
    /// Represents an EloquentObject client that can create connections to specific remote object by known identifier and type.
    /// </summary>
    public interface IEloquent<out T>: IEloquent
        where T : class
    {
        /// <summary>
        /// Creates a connection to remote object.
        /// </summary>
        /// <typeparam name="T">Eloquent contract (an interface which attributed members can be accessed remotely)</typeparam>
        /// <returns>A connection object that provides an access to the remote object</returns>
        [NotNull]
        T Connect();
    }

    /// <summary>
    /// Represents an object that keeps object hosting until it is disposed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IObjectHost<out T> : IEloquent<T>, IDisposable where T : class
    {
        T Object { get; }
    }
}