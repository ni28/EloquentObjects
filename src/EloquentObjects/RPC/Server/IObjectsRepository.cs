using System;
using JetBrains.Annotations;

namespace EloquentObjects.RPC.Server
{
    internal interface IObjectsRepository : IDisposable
    {
        IDisposable Add(string objectId, IObjectAdapter objectAdapter);
        bool TryGetObject(string objectId, [CanBeNull] out IObjectAdapter objectAdapter);
        bool TryGetObjectId(object @object, out string objectId);

        event EventHandler<string> ObjectRemoved;
    }
}