using System;
using System.IO;
using System.Net;

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
        
        public IInputChannel CreateInputChannel(IHostAddress address)
        {
            return new InputChannel(IPAddress.Parse(address.IpAddress), address.Port, _sendTimeout);
        }

        public IOutputChannel CreateOutputChannel(IHostAddress address)
        {
            return new OutputChannel(IPAddress.Parse(address.IpAddress), address.Port, _sendTimeout, _receiveTimeout);
        }

        #endregion
    }
}