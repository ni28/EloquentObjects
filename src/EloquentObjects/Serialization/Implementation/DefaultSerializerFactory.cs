using System;
using System.Collections.Generic;

namespace EloquentObjects.Serialization.Implementation
{
    internal sealed class DefaultSerializerFactory: ISerializerFactory
    {
        #region Implementation of ISerializerFactory

        public ISerializer Create(Type type, IEnumerable<Type> knownTypes)
        {
            return new DefaultSerializer(type, knownTypes);
        }

        #endregion
    }
}