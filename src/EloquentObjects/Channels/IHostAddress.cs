using System.IO;

namespace EloquentObjects.Channels
{
    /// <summary>
    /// Represents a host address.
    /// </summary>
    internal interface IHostAddress
    {
        /// <summary>
        /// Gets an IP address as a string.
        /// </summary>
        string IpAddress { get; }
        
        /// <summary>
        /// Gets a port as an integer.
        /// </summary>
        int Port { get; }
        
        /// <summary>
        /// Writes a serialized host address to given stream.
        /// </summary>
        /// <returns>a string representation of the host address</returns>
        void Write(Stream stream);
    }
}