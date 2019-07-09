using System;
using System.IO;
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
        ///     Occurs when either heartbeat is considered lost or when client sent a terminate sessionMessage.
        /// </summary>
        event EventHandler Terminated;

        /// <summary>
        ///     Handles a sessionMessage from the client.
        /// </summary>
        /// <param name="message">SessionMessage</param>
        /// <param name="stream">Stream that will be used to send a response</param>
        void HandleMessage(Message message, Stream stream);
    }
}