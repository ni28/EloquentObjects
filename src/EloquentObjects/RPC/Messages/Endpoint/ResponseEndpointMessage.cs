using System.IO;
using EloquentObjects.Serialization;

namespace EloquentObjects.RPC.Messages.Endpoint
{
    internal sealed class ResponseEndpointMessage : EndpointMessage
    {
        internal ResponseEndpointMessage(object response)
        {
            Response = response;
        }
        
        public object Response { get; }
        
        #region Overrides of Message

        public override EndpointMessageType MessageType => EndpointMessageType.Response;
        protected override void WriteInternal(Stream stream, ISerializer serializer)
        {
            stream.WritePayload(Serialize(serializer, Response));
        }

        #endregion

        public static EndpointMessage ReadInternal(Stream stream, ISerializer serializer)
        {
            var payload = stream.TakePayload();
            var response = Deserialize(serializer, payload);

            return new ResponseEndpointMessage(response);
        }
    }
}