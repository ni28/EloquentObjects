using System;
using EloquentObjects.Channels;
using EloquentObjects.RPC.Messages.OneWay;

namespace EloquentObjects.RPC.Server
{
    /// <summary>
    /// Represents an object that tracks a subscription to event.
    /// </summary>
    internal interface ISubscription: IDisposable
    {
        /// <summary>
        /// Gets objectId of the remote object that owns event.
        /// </summary>
        string ObjectId { get; }
        
        /// <summary>
        /// Gets the subscribed event name.
        /// </summary>
        string EventName { get; }
        
        /// <summary>
        /// Gets the event handler.
        /// </summary>
        Action<EventMessage> Handler { get; }
        
        /// <summary>
        /// Gets the host address of the client that subscribed to event.
        /// </summary>
        IHostAddress ClientHostAddress { get; }
    }
}