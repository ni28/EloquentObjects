using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages
{
    internal abstract class AcknowledgedMessage: Message
    {
        protected AcknowledgedMessage(IHostAddress clientHostAddress) : base(clientHostAddress)
        {
        }
    }
}