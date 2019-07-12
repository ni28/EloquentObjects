using EloquentObjects.Channels;

namespace EloquentObjects.RPC.Messages
{
    internal abstract class OneWayMessage: Message
    {
        protected OneWayMessage(IHostAddress clientHostAddress) : base(clientHostAddress)
        {
        }
    }
}