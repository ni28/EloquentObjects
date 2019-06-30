using System.Threading;
using EloquentObjects.Channels;
using EloquentObjects.Channels.Implementation;
using EloquentObjects.Contracts;
using EloquentObjects.Contracts.Implementation;
using EloquentObjects.RPC.Server.Implementation;
using EloquentObjects.Serialization;
using EloquentObjects.Serialization.Implementation;
using JetBrains.Annotations;

namespace EloquentObjects
{
    public sealed class EloquentServer : IEloquentServer
    {
        private readonly ISerializerFactory _serializerFactory;
        private readonly IContractDescriptionFactory _contractDescriptionFactory;
        private readonly EndpointHub _endpointHub;
        private readonly IInputChannel _inputChannel;
        private readonly Server _server;

        public EloquentServer(string serverIpPort) : this(serverIpPort, new EloquentSettings())
        {
        }

        public EloquentServer(string serverIpPort, EloquentSettings settings, [CanBeNull] ISerializerFactory serializerFactory=null)
        {
            _serializerFactory = serializerFactory ?? new DefaultSerializerFactory();
            var binding = new TcpBinding(settings.HeartBeatMs, settings.MaxHeartBeatLost, settings.SendTimeout, settings.ReceiveTimeout);

            var serverHostAddress = binding.Parse(serverIpPort);

            _contractDescriptionFactory = new CachedContractDescriptionFactory(new ContractDescriptionFactory());

            _inputChannel = binding.CreateInputChannel(serverHostAddress);

            _endpointHub = new EndpointHub();

            _server = new Server(binding, _inputChannel, _endpointHub);
        }

        #region IDisposable

        public void Dispose()
        {
            _server.Dispose();
            _endpointHub.Dispose();
            _inputChannel.Dispose();
        }

        #endregion

        public void Add<T>(string objectId, T obj, SynchronizationContext synchronizationContext = null)
        {
            var contractDescription = _contractDescriptionFactory.Create(typeof(T));
            
            var knownTypes = contractDescription.GetTypes();
            var serializer = _serializerFactory.Create(typeof(object), knownTypes);

            _endpointHub.AddEndpoint(objectId, new ServiceEndpoint(contractDescription, serializer, synchronizationContext, obj));
        }
    }
}