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

            var attribute = contractType.GetCustomAttribute<EloquentContractAttribute>();

            if (attribute == null)
                throw new InvalidOperationException(
                    $"The provided type does not contain a {nameof(EloquentContractAttribute)} attribute: {contractType}");

            var contract = new ContractDescription(contractType);

            foreach (var methodDescription in GetMethods(contractType))
                contract.AddOperationDescription(methodDescription);
            foreach (var propertyDescription in GetProperties(contractType))
                contract.AddOperationDescription(propertyDescription);
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
                .Where(methodInfo => Attribute.IsDefined(methodInfo, typeof(EloquentMethodAttribute)))
                .Select(info => new MethodDescription(info.Name, info, info.GetCustomAttribute<EloquentMethodAttribute>().IsOneWay))
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
                .Where(info => Attribute.IsDefined(info, typeof(EloquentPropertyAttribute)))
                .Select(info =>
                {
                    var propertyAttribute = info.GetCustomAttribute<EloquentPropertyAttribute>();
                    
                    var getMethodDescription = info.GetMethod == null ? null : new MethodDescription(info.GetMethod.Name, info.GetMethod, propertyAttribute.IsOneWay);
                    var setMethodDescription = info.SetMethod == null ? null : new MethodDescription(info.SetMethod.Name, info.SetMethod, propertyAttribute.IsOneWay);
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