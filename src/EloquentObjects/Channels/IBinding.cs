using System.IO;

namespace EloquentObjects.Channels
{
    /// <summary>
    ///     Represents a combination of protocols, transports, and message encoders used for communication between clients and
    ///     services.
    /// </summary>
    internal interface IBinding
    {
        /// <summary>
        ///     Delay between sending heart beats to keep connection alive.
        /// </summary>
        int HeartBeatMs { get; }

        /// <summary>
        ///     Amount of lost heartbeats that are allowed until connection is considered lost.
        /// </summary>
        int MaxHeartBeatLost { get; }

        /// <summary>
        ///     Reads host address from the stream.
        /// </summary>
        /// <param name="stream">Stream that will return the serialized host address.</param>
        /// <returns>Host address object</returns>
        IHostAddress ReadHostAddress(Stream stream);

        /// <summary>
        ///     Creates an input channel that is used to receive messages and send responses.
        /// </summary>
        /// <param name="address">Base address (e.g. tcp://127.0.0.1) of the input channel</param>
        /// <returns>An input channel object.</returns>
        IInputChannel CreateInputChannel(IHostAddress address);

        /// <summary>
        ///     Creates an output channel that is used to send messages.
        /// </summary>
        /// <param name="address">Base address (e.g. tcp://127.0.0.1) of the output channel</param>
        /// <returns>An output channel object.</returns>
        IOutputChannel CreateOutputChannel(IHostAddress address);
    }
}