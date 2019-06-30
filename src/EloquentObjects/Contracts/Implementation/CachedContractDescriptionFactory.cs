using System;
using System.Collections.Generic;

namespace EloquentObjects.Contracts.Implementation
{
    internal sealed class CachedContractDescriptionFactory : IContractDescriptionFactory
    {
        private readonly IContractDescriptionFactory _contractDescriptionFactory;

        private readonly Dictionary<Type, IContractDescription> _contractDescriptions =
            new Dictionary<Type, IContractDescription>();

        public CachedContractDescriptionFactory(IContractDescriptionFactory contractDescriptionFactory)
        {
            _contractDescriptionFactory = contractDescriptionFactory;
        }

        #region Implementation of IContractDescriptionFactory

        public IContractDescription Create(Type contractType)
        {
            lock (_contractDescriptions)
            {
                if (_contractDescriptions.TryGetValue(contractType, out var contractDescription))
                    return contractDescription;

                contractDescription = _contractDescriptionFactory.Create(contractType);
                _contractDescriptions.Add(contractType, contractDescription);

                return contractDescription;
            }
        }

        #endregion
    }
}