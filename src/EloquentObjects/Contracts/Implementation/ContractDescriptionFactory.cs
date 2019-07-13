using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace EloquentObjects.Contracts.Implementation
{
    internal sealed class ContractDescriptionFactory : IContractDescriptionFactory
    {
        #region Implementation of IContractDescriptionFactory

        public IContractDescription Create(Type contractType)
        {
            if (contractType == null)
                throw new ArgumentNullException(nameof(contractType));
            
            var contract = new ContractDescription(contractType);

            foreach (var methodDescription in GetMethods(contractType))
                contract.AddOperationDescription(methodDescription);
            foreach (var methodDescription in GetProperties(contractType))
                contract.AddOperationDescription(methodDescription);
            foreach (var eventInfo in GetEvents(contractType))
                contract.AddEventDescription(new EventDescription(eventInfo.Name, eventInfo));

            return contract;
        }

        #endregion

        #region Methods

        [NotNull]
        private IEnumerable<IMethodDescription> GetMethods(
            [NotNull] Type contractType)
        {
            return GetMethodsRecursive(contractType)
                .Select(info =>
                {
                    if (info.IsSpecialName)
                        return null;
                    var isOneWay = info.GetCustomAttributes().Any(a => a.GetType().Name.Contains("OneWay"));
                    return new MethodDescription(info.Name, info, isOneWay);
                })
                .Where(i => i != null)
                .ToArray();
        }

        private MethodInfo[] GetMethodsRecursive(Type contractType)
        {
            var contractTypeMethodInfos = contractType.GetMethods();

            foreach (var typeInterface in contractType.GetInterfaces())
                contractTypeMethodInfos = contractTypeMethodInfos.Concat(GetMethodsRecursive(typeInterface)).ToArray();

            return contractTypeMethodInfos;
        }

        #endregion

        #region Properties

        [NotNull]
        private IEnumerable<IMethodDescription> GetProperties(
            [NotNull] Type contractType)
        {
            return GetPropertiesRecursive(contractType)
                .Select(info =>
                {
                    var isOneWay = info.GetCustomAttributes().Any(a => a.GetType().Name.Contains("OneWay"));
                    var getMethodDescription = info.GetMethod == null ? null : new MethodDescription(info.GetMethod.Name, info.GetMethod, isOneWay);
                    var setMethodDescription = info.SetMethod == null ? null : new MethodDescription(info.SetMethod.Name, info.SetMethod, isOneWay);
                    return new IMethodDescription[] {getMethodDescription, setMethodDescription}.Where(md => md != null).ToArray();
                })
                .SelectMany(md => md)
                .ToArray();
        }

        private PropertyInfo[] GetPropertiesRecursive(Type contractType)
        {
            var contractTypePropertyInfos = contractType.GetProperties();

            foreach (var typeInterface in contractType.GetInterfaces())
                contractTypePropertyInfos = contractTypePropertyInfos.Concat(GetPropertiesRecursive(typeInterface)).ToArray();

            return contractTypePropertyInfos;
        }

        #endregion
        
        #region Events

        private IEnumerable<EventInfo> GetEvents(Type contractType)
        {
            return GetEventsRecursive(contractType)
                .Where(eventInfo => Attribute.IsDefined(eventInfo, typeof(EloquentEventAttribute)))
                .ToArray();
        }

        private EventInfo[] GetEventsRecursive(Type contractType)
        {
            var contractTypeEventInfos = contractType.GetEvents();

            foreach (var typeInterface in contractType.GetInterfaces())
                contractTypeEventInfos = contractTypeEventInfos.Concat(GetEventsRecursive(typeInterface)).ToArray();

            return contractTypeEventInfos;
        }

        #endregion
    }
}