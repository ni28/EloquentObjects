using System;

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
        IConnection<T> Connect<T>(string objectId) where T : class;
    }
}