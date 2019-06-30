using System;
using JetBrains.Annotations;

namespace EloquentObjects.Contracts
{
    /// <summary>
    ///     Creates a contract description.
    /// </summary>
    internal interface IContractDescriptionFactory
    {
        [NotNull]
        IContractDescription Create([NotNull] Type contractType);
    }
}