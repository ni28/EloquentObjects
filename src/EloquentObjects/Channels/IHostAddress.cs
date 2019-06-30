using System.IO;

namespace EloquentObjects.Channels
{
    /// <summary>
    ///     Represents a host address.
    /// </summary>
    internal interface IHostAddress
    {
        /// <summary>
        ///     Writes a serialized host address to given stream.
        /// </summary>
        /// <returns>a string representation of the host address</returns>
        void Write(Stream stream);
    }
}