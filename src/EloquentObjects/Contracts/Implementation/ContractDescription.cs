using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace EloquentObjects.Contracts.Implementation
{
    internal sealed class ContractDescription : IContractDescription
    {
        private readonly List<IMethodDescription> _operations = new List<IMethodDescription>();
        private readonly List<IEventDescription> _events = new List<IEventDescription>();
        private readonly Type _contractType;

        public ContractDescription([NotNull] Type contractType)
        {
            _contractType = contractType;
        }

        public void AddOperationDescription(IMethodDescription method)
        {
            _operations.Add(method);
        }


        public void AddEventDescription(IEventDescription ev)
        {
            _events.Add(ev);
        }

        #region Overrides of Object

        public override string ToString()
        {
            return _contractType.ToString();
        }

        #endregion

        #region Implementation of IContractDescription

        public IEnumerable<IEventDescription> Events => _events;

        public IEnumerable<Type> GetTypes()
        {
            var types = new HashSet<Type>();

            foreach (var operationDescription in _operations)
            {
                types.Add(operationDescription.Method.ReturnType);
                foreach (var parameter in operationDescription.Method.GetParameters().Select(parameterInfo => parameterInfo.ParameterType))
                {
                    types.Add(parameter);
                }
            }

            foreach (var eventDescription in Events)
            {
                foreach (var argument in eventDescription.Event.EventHandlerType.GenericTypeArguments)
                {
                    types.Add(argument);
                }
            }

            return types;
        }

        public IMethodDescription GetOperationDescription(string operationName, object[] arguments)
        {
            var operations = _operations.Where(o => o.Name == operationName).ToArray();
            if (operations.Length == 0)
                throw new MissingMethodException(_contractType.FullName, operationName);

            IMethodDescription methodDescription;
            try
            {
                methodDescription = operations.Single(o => OperationWithArguments(o, arguments));
            }
            catch (Exception ex)
            {
                throw new MissingMethodException(
                    $"No operations with name {operationName} and {arguments.Length} arguments were found in {_contractType.FullName}.", ex);
            }

            return methodDescription;
        }

        #endregion
        
        private bool OperationWithArguments(IMethodDescription methodDescription, object[] invocationArguments)
        {
            var parameters = methodDescription.Method.GetParameters();

            return parameters.Length == invocationArguments.Length && parameters.Zip(
                       invocationArguments,
                       (p, ia) =>
                       {
                           if (ia == null)
                           {
                               // Check if null can be assigned to the parameter type
                               var canBeNull = !p.ParameterType.IsValueType
                                               || Nullable.GetUnderlyingType(p.ParameterType) != null;
                               return canBeNull;
                           }

                           return p.ParameterType.IsInstanceOfType(ia);
                       }).All(p => p);
        }
    }
}