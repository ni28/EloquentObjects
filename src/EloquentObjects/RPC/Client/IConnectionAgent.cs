using System;
using System.IO;

namespace EloquentObjects.RPC.Client
{
    internal interface IConnectionAgent : IDisposable
    {
        /// <summary>
        /// Gets the connection ID (server also tracks this connection using the same ID).
        /// </summary>
        int ConnectionId { get; }
        
        /// <summary>
        /// Send one-way message to the server.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="arguments"></param>
        void Notify(string eventName, object[] arguments);

        /// <summary>
        /// Send request and receive response from the server
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        object Call(string methodName, object[] parameters);

        /// <summary>
        /// Occurs when event is received from the server
        /// </summary>
        event EventHandler<NotifyEventArgs> EventReceived;

        void ReceiveAndHandleEndpointMessage(Stream stream);
    }
}