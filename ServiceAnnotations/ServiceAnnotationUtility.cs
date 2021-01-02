using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceAnnotations
{
    public static class ServiceAnnotationUtility
    {
        public static T AddAnnotatedServices<T>(this T serviceCollection) where T : IServiceCollection 
            => serviceCollection.AddAnnotatedServices(Assembly.GetCallingAssembly());

        public static T AddAnnotatedServices<T>(this T serviceCollection, Assembly assembly) where T : IServiceCollection
        {
            foreach (var type in assembly.GetTypes())
                foreach (var attribute in type.GetCustomAttributes<ServiceAttribute>())
                    attribute.Apply(serviceCollection, type);
            
            return serviceCollection;
        }
    }
}