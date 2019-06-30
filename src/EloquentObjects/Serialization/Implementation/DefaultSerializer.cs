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
            types.Add(typeof(CallEnvelope));

            _serializer = new DataContractSerializer(type, types);
        }

        #region Implementation of ISerializer<T>

        public void WriteObject(Stream stream, object obj)
        {
            _serializer.WriteObject(stream, obj);
        }

        public object ReadObject(Stream stream)
        {
            return _serializer.ReadObject(stream);
        }

        public void WriteCall(Stream stream, Call call)
        {
            _serializer.WriteObject(stream, new CallEnvelope(call.OperationName, call.Parameters));
        }

        public Call ReadCall(Stream stream)
        {
            var envelope = (CallEnvelope)_serializer.ReadObject(stream);
            return new Call(envelope.OperationName, envelope.Parameters);
        }

        #endregion
    }
}