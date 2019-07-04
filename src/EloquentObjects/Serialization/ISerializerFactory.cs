using System;
using System.Collections.Generic;

namespace EloquentObjects.Serialization
{
    /// <summary>
    /// Represents a factory that can create a serializer for EloquentServer or EloquentClient.
    /// </summary>
    public interface ISerializerFactory
    {
        /// <summary>
        /// Creates a serializer for EloquentServer or EloquentClient.
        /// </summary>
        /// <param name="type">Type to be serialized</param>
        /// <param name="knownTypes">Types that can be used as members in the serialized Type</param>
        /// <returns>Serializer instance</returns>
        ISerializer Create(Type type, IEnumerable<Type> knownTypes);
    }
}