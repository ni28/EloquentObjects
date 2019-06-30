using System;
using System.Collections.Generic;

namespace EloquentObjects.Serialization
{
    public interface ISerializerFactory
    {
        ISerializer Create(Type type, IEnumerable<Type> knownTypes);
    }
}