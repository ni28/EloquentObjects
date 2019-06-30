using Castle.DynamicProxy;
using EloquentObjects.Channels;
using EloquentObjects.Channels.Implementation;
using EloquentObjects.Contracts.Implementation;
using EloquentObjects.RPC.Client.Implementation;
using EloquentObjects.Serialization;
using EloquentObjects.Serialization.Implementation;
using JetBrains.Annotations;

namespace EloquentObjects
{
    public sealed class EloquentClient : IEloquentClient
    {
        private readonly ISerializerFactory _serializerFactory;
        private readonly CachedContractDescriptionFactory _contractDescriptionFactory;
        private readonly IInputChannel _inputChannel;
        private readonly IOutputChannel _outputChannel;
        private readonly ProxyGenerator _proxyGenerator;
        private readonly SessionAgent _sessionAgent;

        public EloquentClient(string serverIpPort, string clientIpPort) : this(serverIpPort, clientIpPort, new EloquentSettings())
        {
        }
        
        public EloquentClient(string serverIpPort, string clientIpPort, EloquentSettings settings, [CanBeNull] ISerializerFactory serializerFactory=null)
        {
            _serializerFactory = serializerFactory ?? new DefaultSerializerFactory();
            var binding = new TcpBinding(settings.HeartBeatMs, 0, settings.SendTimeout, settings.ReceiveTimeout);
            var serverHostAddress = binding.Parse(serverIpPort);
            var clientHostAddress = binding.Parse(clientIpPort);

            _contractDescriptionFactory = new CachedContractDescriptionFactory(new ContractDescriptionFactory());

            _proxyGenerator = new ProxyGenerator();

            _inputChannel = binding.CreateInputChannel(clientHostAddress);
            _outputChannel = binding.CreateOutputChannel(serverHostAddress);
            _sessionAgent = new SessionAgent(binding, _inputChannel, _outputChannel, clientHostAddress);
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            _sessionAgent.Dispose();
            _outputChannel.Dispose();
            _inputChannel.Dispose();
        }

        #endregion

        public Connection<T> Connect<T>(string objectId) where T : class
        {
            var contractDescription = _contractDescriptionFactory.Create(typeof(T));

            var knownTypes = contractDescription.GetTypes();
            var serializer = _serializerFactory.Create(typeof(object), knownTypes);

            var interceptor = new ClientInterceptor(objectId, _sessionAgent, contractDescription, serializer);
            var proxy = _proxyGenerator.CreateInterfaceProxyWithoutTarget<T>(interceptor);

            return new Connection<T>(interceptor, proxy);
        }
    }
}