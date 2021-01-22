using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceAnnotations
{
    public static class ServiceAnnotationUtility
    {
        public static T AddAnnotatedServices<T>(this T serviceCollection) where T : IServiceCollection 
            => AddAnnotatedServices(serviceCollection, Assembly.GetCallingAssembly());
        
        public static T AddAnnotatedServices<T>(this T serviceCollection, Action<IConfigureAnnotationContext> configure) where T : IServiceCollection 
            => AddAnnotatedServices(serviceCollection, Assembly.GetCallingAssembly(), configure);

        public static T AddAnnotatedServices<T>(this T serviceCollection, Assembly assembly)
            where T : IServiceCollection
            => AddAnnotatedServices(serviceCollection, assembly, _ => { });
        
        public static T AddAnnotatedServices<T>(this T serviceCollection, Assembly assembly, Action<IConfigureAnnotationContext> configure) where T : IServiceCollection
        {
            var contextBuilder = new AnnotationContext.Builder(serviceCollection);
            configure?.Invoke(contextBuilder);
            
            AnnotationContext context = contextBuilder.Build();

            foreach (var type in assembly.GetTypes())
            foreach (var attribute in type.GetCustomAttributes<ServiceAnnotationAttribute>(false))
                attribute.Apply(type, context);
            
            return serviceCollection;
        }
    }
}

