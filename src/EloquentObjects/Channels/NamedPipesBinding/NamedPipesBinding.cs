using System.Net;

namespace EloquentObjects.Channels.NamedPipesBinding
{
    internal sealed class NamedPipesBinding : IBinding
    {
        private readonly int _sendTimeout;
        private readonly int _receiveTimeout;

        public NamedPipesBinding(int heartBeatMs, int maxHeartBeatLost, int sendTimeout, int receiveTimeout)
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
            return new InputChannel($"{address.IpAddress}:{address.Port}");
        }

        public IOutputChannel CreateOutputChannel(IHostAddress address)
        {
            return new OutputChannel($"{address.IpAddress}:{address.Port}", _sendTimeout);
        }

        #endregion
    }
}