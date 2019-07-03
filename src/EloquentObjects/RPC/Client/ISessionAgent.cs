﻿using System;
using EloquentObjects.Serialization;

namespace EloquentObjects.RPC.Client
{
    internal interface ISessionAgent : IDisposable
    {
        IConnectionAgent Connect(string endpointId, ISerializer serializer);

        event EventHandler<EndpointMessageReadyEventArgs> EndpointMessageReady;
    }
}