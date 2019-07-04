using System.IO;
using EloquentObjects.Serialization;

namespace EloquentObjects.RPC.Messages.Endpoint
{
    internal sealed class RequestEndpointMessage : EndpointMessage
    {
        internal RequestEndpointMessage(string endpointId, int connectionId, string methodName, object[] parameters)
        {
            EndpointId = endpointId;
            ConnectionId = connectionId;
            MethodName = methodName;
            Parameters = parameters;
        }

        public string EndpointId { get; }
        public int ConnectionId { get; }
        public string MethodName { get; }
        public object[] Parameters { get; }

        #region Overrides of Message

        public override EndpointMessageType MessageType => EndpointMessageType.Request;
        protected override void WriteInternal(Stream stream, ISerializer serializer)
        {
            stream.WriteString(EndpointId);
            stream.WriteInt(ConnectionId);
            stream.WritePayload(SerializeCall(serializer, new CallInfo(MethodName, Parameters)));
        }

        #endregion

        public static EndpointMessage ReadInternal(Stream stream, ISerializer serializer)
        {
            var endpointId = stream.TakeString();
            var connectionId = stream.TakeInt();
            var payload = stream.TakePayload();
            
            var request = DeserializeCall(serializer, payload);

            return new RequestEndpointMessage(endpointId, connectionId, request.OperationName, request.Parameters);
        }
    }
}