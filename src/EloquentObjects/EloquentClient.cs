using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly IHostAddress _clientHostAddress;
        private readonly List<Tuple<string, WeakReference>> _objectIds = new List<Tuple<string,WeakReference>>();

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
            _clientHostAddress = HostAddress.CreateFromUri(clientUri);

            _contractDescriptionFactory = new CachedContractDescriptionFactory(new ContractDescriptionFactory());

            _proxyGenerator = new ProxyGenerator();

            try
            {
                _inputChannel = binding.CreateInputChannel(_clientHostAddress);
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
            
            _inputChannel.Start();

            //Send HelloMessage to create a session
            var helloMessage = new HelloMessage(_clientHostAddress);
            _outputChannel.SendWithAck(helloMessage);
            
            _eventHandlersRepository = new EventHandlersRepository(_outputChannel, _clientHostAddress, this);
            _sessionAgent = new SessionAgent(binding, _inputChannel, _outputChannel, _clientHostAddress, _eventHandlersRepository);
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
        public T Connect<T>(string objectId) where T : class
        {
            return (T)Connect(typeof(T), objectId);
        }

        /// <inheritdoc />
        public object Connect(Type type, string objectId)
        {
            var contractDescription = _contractDescriptionFactory.Create(type);

            //Send ConnectObjectMessage to ensure that object is hosted
            var connectObjectMessage = new ConnectMessage(_clientHostAddress, objectId);
            _outputChannel.SendWithAck(connectObjectMessage);

            var knownTypes = contractDescription.GetTypes();
            var serializer = _serializerFactory.Create(typeof(object), knownTypes);

            var interceptor = new ClientInterceptor(contractDescription);
            var proxy = _proxyGenerator.CreateInterfaceProxyWithoutTarget(type, interceptor);
            
            var connection = new Connection(objectId, proxy, _eventHandlersRepository, contractDescription, serializer, this, _outputChannel, _clientHostAddress);
            
            interceptor.Subscribe(connection);

            lock (_objectIds)
            {
                RemoveDeadObjectIds();
                _objectIds.Add(new Tuple<string, WeakReference>(objectId, new WeakReference(proxy)));
            }
            return proxy;
        }

        private void RemoveDeadObjectIds()
        {
            foreach (var tuple in _objectIds.ToArray())
            {
                if (!tuple.Item2.IsAlive)
                    _objectIds.Remove(tuple);
            }
        }

        public bool TryGetObjectId(object proxyObject, out string objectId)
        {
            lock (_objectIds)
            {
                RemoveDeadObjectIds();

                var result = _objectIds.FirstOrDefault(t => ReferenceEquals(t.Item2.Target, proxyObject));
                if (result == null)
                {
                    objectId = string.Empty;
                    return false;
                }

                objectId = result.Item1;
                return true;
            }
        }
    }
}