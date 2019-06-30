using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace EloquentObjects.Contracts
{
    /// <summary>
    ///     Represents a contract that specifies what an session communicates to the outside world.
    /// </summary>
    internal interface IContractDescription
    {
        /// <summary>
        ///     Gets the collection of event descriptions associated with the contract.
        /// </summary>
        [NotNull]
        IEnumerable<IEventDescription> Events { get; }

        /// <summary>
        ///     Returns a collection of types used by operations (those types will be used as known types by
        ///     DataContractSerializer).
        /// </summary>
        /// <returns>A collection of types used by operations</returns>
        [NotNull]
        IEnumerable<Type> GetTypes();

        /// <summary>
        /// Returns an method description that matches given method name and arguments.
        /// </summary>
        /// <param name="operationName">Operation name</param>
        /// <param name="arguments">Operation arguments</param>
        /// <returns>The method description</returns>
        /// <exception cref="MissingMemberException">Thrown when method with given name or signature that matches given arguments is not found in the contract description</exception>
        IMethodDescription GetOperationDescription(string operationName, object[] arguments);
    }
}