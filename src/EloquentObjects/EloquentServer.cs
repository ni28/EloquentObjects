using System;
using System.IO;
using System.Threading;
using EloquentObjects.Channels;
using EloquentObjects.Channels.Implementation;
using EloquentObjects.Contracts;
using EloquentObjects.Contracts.Implementation;
using EloquentObjects.RPC.Server;
using EloquentObjects.RPC.Server.Implementation;
using EloquentObjects.Serialization;
using EloquentObjects.Serialization.Implementation;
using JetBrains.Annotations;

namespace EloquentObjects
{
    /// <inheritdoc />
    public sealed class EloquentServer : IEloquentServer
    {
        private readonly ISerializerFactory _serializerFactory;
        private readonly IContractDescriptionFactory _contractDescriptionFactory;
        private readonly IEndpointHub _endpointHub;
        private readonly IInputChannel _inputChannel;
        private readonly Server _server;

        /// <summary>
        /// Creates an EloquentObjects server with default settings and serializer (DataContractSerializer is used).
        /// </summary>
        /// <param name="address">Address of the server that hosts object. Can be prefixed with 'tcp://' for TCP binding or 'pipe://' for Named Pipes binding</param>
        public EloquentServer(string address) : this(address, new EloquentSettings())
        {
        }

        /// <summary>
        /// Creates an EloquentObjects server with ability to specify custom settings and dependencies.
        /// </summary>
        /// <param name="address">Address of the server that hosts object. Can be prefixed with 'tcp://' for TCP binding or 'pipe://' for Named Pipes binding</param>
        /// <param name="settings">Custom settings</param>
        /// <param name="serializerFactory">Factory that can create serializer to be used for serializing/deserializing data sent between server and client</param>
        public EloquentServer(string address, EloquentSettings settings, [CanBeNull] ISerializerFactory serializerFactory=null)
        {
            _serializerFactory = serializerFactory ?? new DefaultSerializerFactory();

            var uri = new Uri(address);
            
            var scheme = uri.GetComponents(UriComponents.Scheme, UriFormat.Unescaped);

            var binding = new BindingFactory().Create(scheme, settings);
            
            var serverHostAddress = HostAddress.CreateFromUri(uri);

            _contractDescriptionFactory = new CachedContractDescriptionFactory(new ContractDescriptionFactory());

            try
            {
                _inputChannel = binding.CreateInputChannel(serverHostAddress);
            }
            catch (Exception e)
            {
                throw new IOException("Failed creating input channel", e);
            }
            
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

        /// <inheritdoc />
        public IDisposable Add<T>(string objectId, T obj, SynchronizationContext synchronizationContext = null)
        {
            var contractDescription = _contractDescriptionFactory.Create(typeof(T));
            
            var knownTypes = contractDescription.GetTypes();
            var serializer = _serializerFactory.Create(typeof(object), knownTypes);

            return _endpointHub.AddEndpoint(objectId, new ServiceEndpoint(contractDescription, serializer, synchronizationContext, obj));
        }
    }
}