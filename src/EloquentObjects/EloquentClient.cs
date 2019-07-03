using System;
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

        public EloquentClient(string serverAddress, string clientAddress) : this(serverAddress, clientAddress, new EloquentSettings())
        {
        }
        
        public EloquentClient(string serverAddress, string clientAddress, EloquentSettings settings, [CanBeNull] ISerializerFactory serializerFactory=null)
        {
            _serializerFactory = serializerFactory ?? new DefaultSerializerFactory();
            
            var serverUri = new Uri(serverAddress);
            var clientUri = new Uri(clientAddress);
            
            var serverScheme = serverUri.GetComponents(UriComponents.Scheme, UriFormat.Unescaped);
            var clientScheme = clientUri.GetComponents(UriComponents.Scheme, UriFormat.Unescaped);

            if (serverScheme != clientScheme)
            {
                throw new ArgumentException("Client Uri scheme should match server Uri scheme");
            }
            
            var binding = new BindingFactory().Create(serverScheme, settings);

            var serverHostAddress = HostAddress.CreateFromUri(serverUri);
            var clientHostAddress = HostAddress.CreateFromUri(clientUri);

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

        public IConnection<T> Connect<T>(string objectId) where T : class
        {
            var contractDescription = _contractDescriptionFactory.Create(typeof(T));

            var knownTypes = contractDescription.GetTypes();
            var serializer = _serializerFactory.Create(typeof(object), knownTypes);

            var innerProxy = new ClientInterceptor(contractDescription);
            var outerProxy = _proxyGenerator.CreateInterfaceProxyWithoutTarget<T>(innerProxy);

            var eventHandlersRepository = new EventHandlersRepository(contractDescription, outerProxy);
            
            return new Connection<T>(objectId, innerProxy, outerProxy, eventHandlersRepository, _sessionAgent, serializer);
        }
    }
}