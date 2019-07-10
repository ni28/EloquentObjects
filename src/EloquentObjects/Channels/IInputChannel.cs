using System;

namespace EloquentObjects.Channels
{
    internal interface IInputChannel : IDisposable
    {
        void Start();
        
        event EventHandler<IInputContext> MessageReady;
    }
}