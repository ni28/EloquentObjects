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
        /// <returns>A proxy object that provides an access to the remote object</returns>
        T Connect<T>([NotNull] string objectId) where T : class;

        /// <summary>
        /// Creates a connection to remote object by it's identifier.
        /// </summary>
        /// <param name="type">Type of eloquent contract</param>
        /// <param name="objectId">Object identifier for which a connection will be created</param>
        /// <returns>A connection object that provides an access to the remote object</returns>
        object Connect(Type type, [NotNull] string objectId);

        /// <summary>
        /// Gets an object ID for the given proxy object. Returns false if the given object is not a remote object.
        /// </summary>
        /// <param name="proxyObject">Proxy object that is asked about ID</param>
        /// <param name="objectId">Object ID that is used to host given object</param>
        /// <returns>True if the object ID was fetched. False otherwise.</returns>
        bool TryGetObjectId(object proxyObject, out string objectId);
    }
}