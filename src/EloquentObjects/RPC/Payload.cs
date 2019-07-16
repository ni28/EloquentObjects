using System;
using System.Collections.Generic;
using EloquentObjects.Channels;
using EloquentObjects.RPC.Messages.OneWay;
using EloquentObjects.Serialization;

namespace EloquentObjects.RPC
{
    /// <summary>
    /// Represents an object that manages the array of parameters (that can includes both remote and DTO objects). The payload can also create/parse messages to transfer contents.
    /// </summary>
    internal sealed class Payload
    {
        private readonly object[] _serializedParameters;
        private readonly string[] _objectIds;
        private readonly bool[] _selector;

        public Payload(object[] serializedParameters, string[] objectIds, bool[] selector)
        {
            _serializedParameters = serializedParameters;
            _objectIds = objectIds;
            _selector = selector;
        }
        
        public static Payload Create(IReadOnlyCollection<object> parameters, Func<object, string> getIdByObject)
        {
            var selectorList = new List<bool>(parameters.Count);
            var serializedParametersList = new List<object>(parameters.Count);
            var objectIdsList = new List<string>(parameters.Count);

            //Split parameters to two groups - remote objects and serializable objects.
            //Use selector to store a flag that indicates if a parameter is a remote object.
            foreach (var parameter in parameters)
            {
                var objectId = getIdByObject(parameter);
                if (objectId != null)
                {
                    objectIdsList.Add(objectId);
                    selectorList.Add(true);
                }
                else
                {
                    serializedParametersList.Add(parameter);
                    selectorList.Add(false);
                }
            }

            var serializedParameters = serializedParametersList.ToArray();
            var objectIds = objectIdsList.ToArray();
            var selector = selectorList.ToArray();
            
            return new Payload(serializedParameters, objectIds, selector);
        }

        public NotificationMessage CreateNotificationMessage(ISerializer serializer, IHostAddress clientHostAddress,
            string objectId, string methodName)
        {
            var payload = serializer.Serialize(_serializedParameters);
            return new NotificationMessage(clientHostAddress, objectId, methodName, payload, _selector, _objectIds);
        }

        public RequestMessage CreateRequestMessage(ISerializer serializer, IHostAddress clientHostAddress, string objectId, string methodName)
        {
            var payload = serializer.Serialize(_serializedParameters);

            return new RequestMessage(clientHostAddress, objectId, methodName, payload, _selector, _objectIds);
        }


        public EventMessage CreateEventMessage(ISerializer serializer, IHostAddress clientHostAddress, string objectId, string eventName)
        {
            var payload = serializer.Serialize(_serializedParameters);

            return new EventMessage(clientHostAddress, objectId, eventName, payload, _selector, _objectIds);
        }

        public object[] ToParameters(Func<string, int, object> getObjectById)
        {
            var serializedParametersEnumerator = _serializedParameters.GetEnumerator();
            var objectIdsEnumerator = _objectIds.GetEnumerator();
            serializedParametersEnumerator.MoveNext();
            objectIdsEnumerator.MoveNext();

            //TODO: check consistency

            var parameters = new List<object>(_selector.Length);

            var index = 0;
            foreach (var isRemoteObject in _selector)
            {
                if (isRemoteObject)
                {
                    var objectId = (string) objectIdsEnumerator.Current;
                    var obj = getObjectById(objectId, index);
                    
                    if (obj == null)
                    {
                        throw new ArgumentException($"Object with ID '{objectId}' is not hosted");
                    }

                    parameters.Add(obj);

                    objectIdsEnumerator.MoveNext();
                }
                else
                {
                    parameters.Add(serializedParametersEnumerator.Current);
                    serializedParametersEnumerator.MoveNext();
                }

                index++;
            }

            return parameters.ToArray();
        }
        
        public object[] ToParametersNoCheck(Func<string, int, object> getObjectById)
        {
            var serializedParametersEnumerator = _serializedParameters.GetEnumerator();
            var objectIdsEnumerator = _objectIds.GetEnumerator();
            serializedParametersEnumerator.MoveNext();
            objectIdsEnumerator.MoveNext();

            //TODO: check consistency

            var parameters = new List<object>(_selector.Length);

            var index = 0;
            foreach (var isRemoteObject in _selector)
            {
                if (isRemoteObject)
                {
                    var objectId = (string) objectIdsEnumerator.Current;
                    var obj = getObjectById(objectId, index);

                    parameters.Add(obj);

                    objectIdsEnumerator.MoveNext();
                }
                else
                {
                    parameters.Add(serializedParametersEnumerator.Current);
                    serializedParametersEnumerator.MoveNext();
                }

                index++;
            }

            return parameters.ToArray();

        }
    }
}