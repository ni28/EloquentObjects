using System;
using EloquentObjects.Channels;
using EloquentObjects.RPC.Messages;

namespace EloquentObjects.RPC.Server
{
    /// <summary>
    ///     Represents an object that handles messages for specific client.
    /// </summary>
    internal interface ISession : IDisposable
    {
        /// <summary>
        ///     Gets a host address of the Client that is handled by this session.
        /// </summary>
        IHostAddress ClientHostAddress { get; }

        /// <summary>
        ///     Occurs when either heartbeat is considered lost or when client sent a terminate message.
        /// </summary>
        event EventHandler Terminated;

        /// <summary>
        ///     Handles a message from the client.
        /// </summary>
        /// <param name="message">Message from client</param>
        /// <param name="context">Context that will be used to send a response</param>
        void HandleMessage(Message message, IInputContext context);
    }
}