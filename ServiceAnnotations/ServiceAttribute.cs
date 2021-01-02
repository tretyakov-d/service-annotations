using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceAnnotations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceAttribute : Attribute
    {
        private readonly ServiceLifetime _lifetime;
        private readonly HashSet<Type> _useAs = new HashSet<Type>();

        public ServiceAttribute(ServiceLifetime lifetime, params Type[] useAs)
        {
            _lifetime = lifetime;

            foreach (var type in useAs)
                _useAs.Add(type);
        }

        internal void Apply(IServiceCollection serviceCollection, Type implementationType)
        {
            var primaryType = _useAs.Count == 0 || _useAs.Contains(implementationType)
                ? implementationType
                : _useAs.First();

            serviceCollection.Add(new ServiceDescriptor(primaryType, implementationType, _lifetime));

            foreach (Type mappedType in _useAs.Where(t => t != primaryType))
                serviceCollection.Add(new ServiceDescriptor(mappedType, sp => sp.GetService(primaryType), _lifetime));
        }
    }
}