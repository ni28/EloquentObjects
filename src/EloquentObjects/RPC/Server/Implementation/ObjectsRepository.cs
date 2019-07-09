using System;
using System.Collections.Generic;
using EloquentObjects.Channels;

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

        public bool TryConnectObject(string objectId, IHostAddress clientHostAddress, int connectionId,
            IOutputChannel outputChannel, out IConnection connection)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ObjectsRepository));

            connection = null;
            IObjectAdapter objectAdapter;
            lock (_objects)
            {
                if (!_objects.TryGetValue(objectId, out objectAdapter))
                    return false;
            }
            
            connection = objectAdapter.Connect(objectId, clientHostAddress, connectionId, outputChannel);
            return true;
        }

        public IDisposable Add(string objectId, IObjectAdapter objectAdapter)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ObjectsRepository));

            lock (_objects)
            {
                _objects.Add(objectId, objectAdapter);
            }
            
            return new Disposable(() => Remove(objectId));
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