using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceAnnotations
{
    /// <summary>
    ///     Marks service to be registered by serviceCollection.AddAnnotatedServices extension should register the class 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceAttribute : ServiceAnnotationAttribute
    {
        readonly ServiceLifetime _lifetime;
        readonly HashSet<Type> _useAs = new();

        /// <summary>
        ///     Marks service to be registered by serviceCollection.AddAnnotatedServices extension should register the class 
        /// </summary>
        /// <param name="lifetime">
        ///     Defines service <see cref="T:Microsoft.Extensions.DependencyInjection.ServiceLifetime" /> for registration.
        /// </param>
        /// <param name="useAs">
        ///     Defines service type(s). In other words the key(s) for resolving service from DI container.
        ///     Defaults to implementation type (the class being applied to) if not specified.
        /// .</param>
        public ServiceAttribute(ServiceLifetime lifetime, params Type[] useAs)
        {
            _lifetime = lifetime;

            foreach (var type in useAs)
                _useAs.Add(type);
        }

        internal override void Apply(Type implementationType, IAnnotationContext context)
        {
            var primaryType = _useAs.Count == 0 || _useAs.Contains(implementationType)
                ? implementationType
                : _useAs.First();

            IServiceCollection serviceCollection = context.TargetCollection;

            serviceCollection.Add(new ServiceDescriptor(primaryType, implementationType, _lifetime));

            foreach (Type mappedType in _useAs.Where(t => t != primaryType))
                serviceCollection.Add(new ServiceDescriptor(mappedType, sp => sp.GetService(primaryType), _lifetime));
        }
    }
}