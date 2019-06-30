using System;
using System.IO;
using System.Net;

namespace EloquentObjects.Channels.Implementation
{
    internal sealed class TcpHostAddress : ITcpHostAddress, IEquatable<ITcpHostAddress>
    {
        private TcpHostAddress(IPAddress ipAddress, int port)
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
            stream.WriteString(IpAddress.ToString());
            stream.WriteInt(Port);
        }

        #endregion

        public static TcpHostAddress Read(Stream stream)
        {
            var ipAddress = stream.TakeString();
            var port = stream.TakeInt();
            return new TcpHostAddress(IPAddress.Parse(ipAddress), port);
        }
        
        public static TcpHostAddress Parse(string hostAddress)
        {
            var index = hostAddress.LastIndexOf(':');
            if (index < 0)
                throw new FormatException(
                    $"The host address string does not match expected format: {hostAddress} (format <IP address>:<port> was expected)");

            var ipAddress = hostAddress.Substring(0, index);
            var strPort = hostAddress.Substring(index + 1);

            if (!int.TryParse(strPort, out var port))
                throw new FormatException($"The port of the host address is not an integer: {hostAddress}");

            return new TcpHostAddress(IPAddress.Parse(ipAddress), port);        }
        
        #region Implementation of ITcpHostAddress

        public IPAddress IpAddress { get; }

        public int Port { get; }

        #endregion

        #region Equality members

        private bool Equals(TcpHostAddress other)
        {
            return Equals(IpAddress, other.IpAddress) && Port == other.Port;
        }

        public bool Equals(ITcpHostAddress other)
        {
            return Equals(IpAddress, other?.IpAddress) && Port == other?.Port;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is TcpHostAddress other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((IpAddress != null ? IpAddress.GetHashCode() : 0) * 397) ^ Port;
            }
        }

        #endregion
    }
}