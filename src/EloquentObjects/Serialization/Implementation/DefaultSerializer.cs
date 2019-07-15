using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace EloquentObjects.Serialization.Implementation
{
    internal sealed class DefaultSerializer: ISerializer
    {
        private readonly DataContractSerializer _serializer;

        public DefaultSerializer(Type type, IEnumerable<Type> knownTypes)
        {
            var types = knownTypes.ToList();
            types.Add(typeof(Envelope));

            _serializer = new DataContractSerializer(type, types);
        }

        #region Implementation of ISerializer<T>

        public void WriteObjects(Stream stream, object[] objects)
        {
            _serializer.WriteObject(stream, new Envelope(objects));
        }

        public object[] ReadObjects(Stream stream)
        {
            return ((Envelope)_serializer.ReadObject(stream)).Parameters;
        }

        #endregion
    }
}