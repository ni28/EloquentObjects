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
            if (Response is IEloquent eloquent)
            {
                stream.WriteBool(true);
                stream.WriteString(eloquent.ObjectId);
                stream.WritePayload(Serialize(serializer, eloquent.Info));
            }
            else
            {
                stream.WriteBool(false);
                stream.WritePayload(Serialize(serializer, Response));
            }
        }

        #endregion

        public static EndpointMessage ReadInternal(Stream stream, ISerializer serializer)
        {
            var isEloquent = stream.TakeBool();

            if (isEloquent)
            {
                var objectId = stream.TakeString();
                var info = Deserialize(serializer, stream.TakePayload());
                return new ResponseEndpointMessage(new EloquentObjectInfo(objectId, info));
            }
            else
            {
                var payload = stream.TakePayload();
                var response = Deserialize(serializer, payload);
                return new ResponseEndpointMessage(response);
            }
        }

        private sealed class EloquentObjectInfo : IEloquent
        {
            public EloquentObjectInfo(string objectId, object info)
            {
                ObjectId = objectId;
                Info = info;
            }

            #region Implementation of IEloquent

            public string ObjectId { get; }
            public object Info { get; }

            #endregion
        }
    }
}