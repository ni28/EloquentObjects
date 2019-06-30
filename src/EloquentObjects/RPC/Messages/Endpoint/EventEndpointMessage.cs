using System.IO;
using EloquentObjects.Serialization;

namespace EloquentObjects.RPC.Messages.Endpoint
{
    internal sealed class EventEndpointMessage : EndpointMessage
    {
        public EventEndpointMessage(string endpointId, int connectionId, string eventName, object[] arguments)
        {
            EndpointId = endpointId;
            ConnectionId = connectionId;
            EventName = eventName;
            Arguments = arguments;
        }

        public string EndpointId { get; }
        public int ConnectionId { get; }
        public string EventName { get; }
        public object[] Arguments { get; }

        #region Overrides of Message

        public override EndpointMessageType MessageType => EndpointMessageType.Event;
        
        protected override void WriteInternal(Stream stream, ISerializer serializer)
        {
            stream.WriteString(EndpointId);
            stream.WriteInt(ConnectionId);
            stream.WritePayload(SerializeCall(serializer, new Call(EventName, Arguments)));
        }

        #endregion

        public static EndpointMessage ReadInternal(Stream stream, ISerializer serializer)
        {
            var endpointId = stream.TakeString();
            var connectionId = stream.TakeInt();
            var payload = stream.TakePayload();
            var call = DeserializeCall(serializer, payload);

            return new EventEndpointMessage(endpointId, connectionId, call.OperationName, call.Parameters);
        }
    }
}