using System;
using System.Collections.Generic;
using System.Linq;

namespace EloquentObjects.RPC.Server.Implementation
{
    internal sealed class ObjectsRepository : IObjectsRepository
    {
        private readonly Dictionary<string, IObjectAdapter> _objects = new Dictionary<string, IObjectAdapter>();
        private bool _disposed;

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ObjectsRepository));
            _disposed = true;
            lock (_objects)
            {
                foreach (var objectAdapter in _objects.Values) objectAdapter.Dispose();
            }
        }

        #endregion

        #region Implementation of IObjectsRepository

        public IDisposable Add(string objectId, IObjectAdapter objectAdapter)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ObjectsRepository));

            lock (_objects)
            {
                _objects.Add(objectId, objectAdapter);
            }
            
            return new Disposable(() => Remove(objectId));
        }

        public bool TryGetObject(string objectId, out IObjectAdapter objectAdapter)
        {
            lock (_objects)
            {
                return _objects.TryGetValue(objectId, out objectAdapter);
            }
        }

        public bool TryGetObjectId(object result, out string objectId)
        {
            lock (_objects)
            {
                var obj = _objects.FirstOrDefault(o => ReferenceEquals(o.Value.Object, result));
                if (string.IsNullOrEmpty(obj.Key))
                {
                    objectId = string.Empty;
                    return false;
                }

                objectId = obj.Key;
                return true;
            }
        }

        #endregion

        private void Remove(string objectId)
        {
            lock (_objects)
            {
                _objects.Remove(objectId);
            }
        }
    }
}