using System;
using EloquentObjects.Channels;
using EloquentObjects.Channels.Implementation;
using EloquentObjects.Channels.NamedPipesBinding;

namespace EloquentObjects
{
    internal sealed class BindingFactory
    {
        public IBinding Create(string scheme, EloquentSettings settings)
        {
            switch (scheme)
            {
                case "tcp":
                    return new TcpBinding(settings.HeartBeatMs, settings.MaxHeartBeatLost, settings.SendTimeout, settings.ReceiveTimeout);
                case "pipe":
                    return new NamedPipesBinding(settings.HeartBeatMs, settings.MaxHeartBeatLost, settings.SendTimeout, settings.ReceiveTimeout);
                default:
                    throw new NotSupportedException($"Scheme is not supported: {scheme}");                        
            }
        }
    }
}