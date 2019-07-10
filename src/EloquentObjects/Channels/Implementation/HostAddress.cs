using System;

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

        public void Write(IFrameBuilder builder)
        {
            builder.WriteString(IpAddress);
            builder.WriteInt(Port);
        }

        #endregion

        public static HostAddress Read(IFrame frame)
        {
            var ipAddress = frame.TakeString();
            var port = frame.TakeInt();
            return new HostAddress(ipAddress, port);
        }
        
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


            var portComponent = baseAddress.GetComponents(UriComponents.Port, UriFormat.Unescaped);
            
            int port;

            if (string.IsNullOrEmpty(portComponent))
            {
                port = -1;
            }
            else if (!int.TryParse(portComponent, out port))
            {
                throw new ArgumentOutOfRangeException(nameof(baseAddress), "Port has invalid format (integer was expected)");
            }
            
            var host = baseAddress.GetComponents(UriComponents.Host, UriFormat.Unescaped);

            return new HostAddress(host, port);
        }
    }
}