using System;
using EloquentObjects.Contracts;
using EloquentObjects.RPC.Messages.OneWay;

namespace EloquentObjects.RPC.Client
{
    internal interface IConnectionAgent : IDisposable
    {
        /// <summary>
        /// Send one-way message to the server.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="arguments"></param>
        void Notify(string eventName, object[] arguments);

        /// <summary>
        /// Send request and receive response from the server
        /// </summary>
        /// <param name="eloquentClient"></param>
        /// <param name="contractDescription"></param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        object Call(IEloquentClient eloquentClient, IContractDescription contractDescription, string methodName,
            object[] parameters);
    }
}