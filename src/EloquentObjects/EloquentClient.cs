using System;
using System.IO;
using Castle.DynamicProxy;
using EloquentObjects.Channels;
using EloquentObjects.Channels.Implementation;
using EloquentObjects.Contracts.Implementation;
using EloquentObjects.RPC;
using EloquentObjects.RPC.Client;
using EloquentObjects.RPC.Client.Implementation;
using EloquentObjects.RPC.Messages.Acknowledged;
using EloquentObjects.Serialization;
using EloquentObjects.Serialization.Implementation;
using JetBrains.Annotations;

namespace EloquentObjects
{
    /// <summary>
    /// Represents an EloquentObjects client that can create connections to remote objects by their identifiers.
    /// </summary>
    public sealed class EloquentClient : IEloquentClient
    {
        private readonly ISerializerFactory _serializerFactory;
        private readonly CachedContractDescriptionFactory _contractDescriptionFactory;
        private readonly IInputChannel _inputChannel;
        private readonly IOutputChannel _outputChannel;
        private readonly ProxyGenerator _proxyGenerator;
        private readonly SessionAgent _sessionAgent;
        private readonly IEventHandlersRepository _eventHandlersRepository;
        private bool _disposed;

        /// <summary>
        /// Creates an EloquentObjects client with default settings and serializer (DataContractSerializer is used).
        /// </summary>
        /// <param name="serverAddress">Address of the server that hosts object. Can be prefixed with 'tcp://' for TCP binding or 'pipe://' for Named Pipes binding</param>
        /// <param name="clientAddress">Client-side address that is used to send server-to-client events. Can be prefixed with 'tcp://' for TCP binding or 'pipe://' for Named Pipes binding</param>
        /// <exception cref="ArgumentException">Client Uri scheme should match server Uri scheme</exception>
        public EloquentClient(string serverAddress, string clientAddress) : this(serverAddress, clientAddress, new EloquentSettings())
        {
        }
        
        /// <summary>
        /// Creates an EloquentObjects client with ability to specify custom settings and dependencies.
        /// </summary>
        /// <param name="serverAddress">Address of the server that hosts object. Can be prefixed with 'tcp://' for TCP binding or 'pipe://' for Named Pipes binding</param>
        /// <param name="clientAddress">Client-side address that is used to send server-to-client events. Can be prefixed with 'tcp://' for TCP binding or 'pipe://' for Named Pipes binding</param>
        /// <param name="settings">Custom settings</param>
        /// <param name="serializerFactory">Factory that can create serializer to be used for serializing/deserializing data sent between server and client</param>
        /// <exception cref="ArgumentException">Client Uri scheme should match server Uri scheme</exception>
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

            try
            {
                _inputChannel = binding.CreateInputChannel(clientHostAddress);
            }
            catch (Exception e)
            {
                throw new IOException("Failed creating input channel", e);
            }

            try
            {
                _outputChannel = binding.CreateOutputChannel(serverHostAddress);
            }
            catch (Exception e)
            {
                throw new IOException("Connection failed. Server not found.", e);
            }
            
            _eventHandlersRepository = new EventHandlersRepository(_outputChannel, clientHostAddress);
            _sessionAgent = new SessionAgent(binding, _inputChannel, _outputChannel, clientHostAddress, _eventHandlersRepository);

            //Send HelloMessage to create a session
            var helloMessage = new HelloMessage(clientHostAddress);
            _outputChannel.SendWithAck(helloMessage);
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(EloquentClient));

            _disposed = true;
            
            _sessionAgent.Dispose();
            _eventHandlersRepository.Dispose();
            _outputChannel.Dispose();
            _inputChannel.Dispose();
        }

        #endregion

        /// <inheritdoc />
        public T Get<T>(string objectId) where T : class
        {
            return (T)Get(typeof(T), objectId);
        }

        public object Get(Type type, string objectId)
        {
            var contractDescription = _contractDescriptionFactory.Create(type);

            var knownTypes = contractDescription.GetTypes();
            var serializer = _serializerFactory.Create(typeof(object), knownTypes);

            var interceptor = new ClientInterceptor(contractDescription);

            var proxy = _proxyGenerator.CreateInterfaceProxyWithoutTarget(type, interceptor);
            var connection = new Connection(objectId, proxy, _sessionAgent, _eventHandlersRepository, contractDescription, serializer, this);
            
            interceptor.Subscribe(connection);

            return proxy;
        }
    }
}