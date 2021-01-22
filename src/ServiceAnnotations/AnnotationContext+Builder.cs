using System.Collections;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceAnnotations
{
    public interface IConfigureAnnotationContext
    {
        /// <summary>
        ///     Adds service to be available as a parameter for ConfigureServices attribute targeted methods
        /// </summary>
        /// <para name="instance">An instance of a service.</para>
        /// <typeparam name="TService">
        ///     Defines the type of parameter for which the instance should be used.
        ///     Typically it is an interface, like IConfiguration or IHostingEnvironment, instead of explicit types.
        /// </typeparam>
        public IConfigureAnnotationContext Add<TService>(TService instance) where TService : class;
    }

    partial class AnnotationContext
    {
        public class Builder : IConfigureAnnotationContext
        {
            readonly IServiceCollection _targetCollection;
            readonly Hashtable _contextServices;

            public Builder(IServiceCollection targetCollection)
            {
                _targetCollection = targetCollection;
                _contextServices = new Hashtable
                {
                    {typeof(IServiceCollection), _targetCollection},
                    {targetCollection.GetType(), _targetCollection}
                };
            }

            public IConfigureAnnotationContext Add<TService>(TService instance) where TService : class
            {
                _contextServices[typeof(TService)] = instance;
                return this;
            }

            public AnnotationContext Build() => new(_targetCollection, _contextServices);
        }
    }
}