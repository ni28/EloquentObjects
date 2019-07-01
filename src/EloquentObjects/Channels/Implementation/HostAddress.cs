using System;
using System.IO;

namespace EloquentObjects.Channels.Implementation
{
    internal sealed class HostAddress : IHostAddress, IEquatable<IHostAddress>
    {
        private HostAddress(string ipAddress, int port)
        {
            IpAddress = ipAddress;
            Port = port;
        }

        #region Overrides of Object

        public override string ToString()
        {
            return $"{IpAddress}:{Port}";
        }

        #endregion

        #region Implementation of IHostAddress

        public void Write(Stream stream)
        {
            stream.WriteString(IpAddress);
            stream.WriteInt(Port);
        }

        #endregion

        public static HostAddress Read(Stream stream)
        {
            var ipAddress = stream.TakeString();
            var port = stream.TakeInt();
            return new HostAddress(ipAddress, port);
        }
        
        public static HostAddress Parse(string hostAddress)
        {
            var index = hostAddress.LastIndexOf(':');
            if (index < 0)
                throw new FormatException(
                    $"The host address string does not match expected format: {hostAddress} (format <IP address>:<port> was expected)");

            var ipAddress = hostAddress.Substring(0, index);
            var strPort = hostAddress.Substring(index + 1);

            if (!int.TryParse(strPort, out var port))
                throw new FormatException($"The port of the host address is not an integer: {hostAddress}");

            return new HostAddress(ipAddress, port);        }
        
        #region Implementation of ITcpHostAddress

        public string IpAddress { get; }

        public int Port { get; }

        #endregion

        #region Equality members

        private bool Equals(HostAddress other)
        {
            return Equals(IpAddress, other.IpAddress) && Port == other.Port;
        }

        public bool Equals(IHostAddress other)
        {
            return Equals(IpAddress, other?.IpAddress) && Port == other?.Port;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is HostAddress other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((IpAddress != null ? IpAddress.GetHashCode() : 0) * 397) ^ Port;
            }
        }

        #endregion

        public static IHostAddress CreateFromUri(Uri baseAddress)
        {
            var path = baseAddress.GetComponents(UriComponents.Path, UriFormat.Unescaped);
            if (!string.IsNullOrEmpty(path))
            {
                throw new ArgumentOutOfRangeException(nameof(baseAddress), "Uri should not have a path");
            }
            
            if (!int.TryParse(baseAddress.GetComponents(UriComponents.Port, UriFormat.Unescaped), out var port))
            {
                throw new ArgumentOutOfRangeException(nameof(baseAddress), "Port has invalid format (integer was expected)");
            }
            
            var host = baseAddress.GetComponents(UriComponents.Host, UriFormat.Unescaped);

            return new HostAddress(host, port);
        }
    }
}