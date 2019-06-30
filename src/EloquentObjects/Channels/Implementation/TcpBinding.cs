using System;
using System.IO;

namespace EloquentObjects.Channels.Implementation
{
    internal sealed class TcpBinding : IBinding
    {
        private readonly int _sendTimeout;
        private readonly int _receiveTimeout;

        public TcpBinding(int heartBeatMs, int maxHeartBeatLost, int sendTimeout, int receiveTimeout)
        {
            HeartBeatMs = heartBeatMs;
            MaxHeartBeatLost = maxHeartBeatLost;
            _sendTimeout = sendTimeout;
            _receiveTimeout = receiveTimeout;
        }

        #region Implementation of IBinding

        public int HeartBeatMs { get; }
        public int MaxHeartBeatLost { get; }

        public IHostAddress ReadHostAddress(Stream stream)
        {
            return TcpHostAddress.Read(stream);
        }

        public IHostAddress Parse(string hostAddress)
        {
            return TcpHostAddress.Parse(hostAddress);
        }
        
        public IInputChannel CreateInputChannel(IHostAddress address)
        {
            if (address is ITcpHostAddress tcpAddress)
                return new InputChannel(tcpAddress.IpAddress, tcpAddress.Port, _sendTimeout);

            throw new ArgumentException("Incompatible host address type");
        }

        public IOutputChannel CreateOutputChannel(IHostAddress address)
        {
            if (address is ITcpHostAddress tcpAddress)
                return new OutputChannel(tcpAddress.IpAddress, tcpAddress.Port, _sendTimeout, _receiveTimeout);

            throw new ArgumentException("Incompatible host address type");
        }

        #endregion
    }
}