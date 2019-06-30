using System;
using System.IO;

namespace EloquentObjects.Channels
{
    internal interface IInputChannel : IDisposable
    {
        void Start();
        event EventHandler<Stream> MessageReady;
    }
}