using System;
using JetBrains.Annotations;

namespace EloquentObjects
{
    /// <summary>
    /// Represents an EloquentObjects client that can create connections to remote objects by their identifiers.
    /// </summary>
    public interface IEloquentClient: IDisposable
    {
        /// <summary>
        /// Creates a connection to remote object by it's identifier.
        /// </summary>
        /// <param name="objectId">Object identifier for which a connection will be created</param>
        /// <typeparam name="T">Eloquent contract (an interface which attributed members can be accessed remotely)</typeparam>
        /// <returns>A connection object that provides an access to the remote object</returns>
        T Connect<T>([NotNull] string objectId) where T : class;

        /// <summary>
        /// Creates a connection to remote object by it's identifier.
        /// </summary>
        /// <param name="type">Type of eloquent contract</param>
        /// <param name="objectId">Object identifier for which a connection will be created</param>
        /// <returns>A connection object that provides an access to the remote object</returns>
        object Connect(Type type, [NotNull] string objectId);
    }
}