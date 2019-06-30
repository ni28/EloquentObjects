using System;
using System.IO;

namespace EloquentObjects.Channels
{
    internal interface IOutputChannelContext: IDisposable
    {
        Stream Stream { get; }
    }
}