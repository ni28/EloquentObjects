using System;
using System.Reflection;
using JetBrains.Annotations;

namespace EloquentObjects.Contracts.Implementation
{
    internal sealed class MethodDescription : IMethodDescription
    {
        public MethodDescription(
            [NotNull] string name,
            [NotNull] MethodInfo method,
            bool isOneWay)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (method == null)
                throw new ArgumentNullException(nameof(method));

            Name = name;
            Method = method;
            IsOneWay = isOneWay;
        }

        #region Implementation of IMethodDescription

        public string Name { get; }

        public MethodInfo Method { get; }
        public bool IsOneWay { get; }
        
        public Type GetEloquentObjectType()
        {
            if (Method.ReturnType.GetGenericTypeDefinition() != typeof(IEloquent<>))
                throw new InvalidOperationException("Return type is not IEloquent<T>");
            return Method.ReturnType.GenericTypeArguments[0];
        }

        #endregion
    }
}