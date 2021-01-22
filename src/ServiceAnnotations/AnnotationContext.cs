using System;
using System.Collections;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceAnnotations
{
    interface IAnnotationContext
    {
        public IServiceCollection TargetCollection { get; }

        object GetService(Type serviceType);
    }

    partial class AnnotationContext : IAnnotationContext
    {
        readonly Hashtable _instances;

        public IServiceCollection TargetCollection { get; }

        public object GetService(Type serviceType) => _instances[serviceType];

        AnnotationContext(IServiceCollection targetCollection, Hashtable instances)
        {
            TargetCollection = targetCollection;
            _instances = instances;
        }
    }
}