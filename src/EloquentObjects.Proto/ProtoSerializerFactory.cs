using System;
using System.Collections.Generic;
using EloquentObjects.Serialization;

namespace EloquentObjects.Proto
{
    internal sealed class ProtoSerializerFactory: ISerializerFactory
    {
        #region Implementation of ISerializerFactory

        public ISerializer Create(Type type, IEnumerable<Type> knownTypes)
        {
            return new ProtoSerializer();
        }

        #endregion
    }
}