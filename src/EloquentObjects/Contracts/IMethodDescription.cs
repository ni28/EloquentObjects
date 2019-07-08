using System;
using System.Reflection;
using JetBrains.Annotations;

namespace EloquentObjects.Contracts
{
    /// <summary>
    ///     Represents the description of a contract method that provides a description of the messages that make up the
    ///     method.
    /// </summary>
    internal interface IMethodDescription
    {
        /// <summary>
        ///     Gets the name of the method description.
        /// </summary>
        [NotNull]
        string Name { get; }

        /// <summary>
        ///     Gets the method that performs the contract method.
        /// </summary>
        MethodInfo Method { get; }
        
        /// <summary>
        /// Indicates that the remote call of this method will not return any result (exceptions will be hidden as well).
        /// </summary>
        bool IsOneWay { get; }
        
        /// <summary>
        /// Returns T type for method which result is IEloquent{T}.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Return type is not IEloquent{T}</exception>
        Type GetEloquentObjectType();

    }
}