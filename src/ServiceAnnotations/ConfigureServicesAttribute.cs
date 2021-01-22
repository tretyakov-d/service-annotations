using System;
using System.Reflection;

namespace ServiceAnnotations
{
    /// <summary>
    ///     Indicates that class have a static method that should be invoked by seriviceCollection.AddAnnoatedServices()
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ConfigureServicesAttribute : ServiceAnnotationAttribute
    {
        const string DefaultMethodName = "ConfigureServices";

        string MethodName { get; }

        /// <summary>
        ///     Indicates that class have a static method that should be invoked by seriviceCollection.AddAnnoatedServices()
        /// </summary>
        /// <param name="methodName">Specifies method name, defaults to "ConfigureServices"</param>
        public ConfigureServicesAttribute(string methodName = DefaultMethodName) => MethodName = methodName;

        internal override void Apply(Type implementationType, IAnnotationContext context)
        {
            MethodInfo method = ResolveMethod(implementationType);

            ParameterInfo[] definitions = method.GetParameters();
            var parameters = new object[definitions.Length];

            for (int i = 0; i < definitions.Length; i++)
                parameters[i] = ResolveParameter(context, definitions[i]);

            method.Invoke(null, parameters);
        }

        object ResolveParameter(IAnnotationContext context, ParameterInfo parameterInfo)
            => context.GetService(parameterInfo.ParameterType)
               ?? throw new ServiceAnnotationException(
                   "SA2002",
                   $"The instance of {parameterInfo.ParameterType.FullName} was not provided. " +
                   $"Required for parameter {parameterInfo.Name} in " +
                   $"{parameterInfo.Member.DeclaringType!.FullName}.{parameterInfo.Member.Name}",
                   "Make sure to add instance to annotation context. " +
                   "For example serviceCollection.AddAnnotatedService(" +
                   $"context => context.Add<{parameterInfo.ParameterType.FullName}>(instance))");

        MethodInfo ResolveMethod(Type implementationType)
        {
            try
            {
                return implementationType.GetMethod(
                           MethodName,
                           BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                       ?? throw new ServiceAnnotationException(
                           "SA2000",
                           $"Configure services method not found by name: {GetMethodNameForErrorMessage()}",
                           "Make sure the method exists and is static");
            }
            catch (AmbiguousMatchException e)
            {
                throw new ServiceAnnotationException(
                    "SA2001",
                    $"Ambiguous configure services method name: {GetMethodNameForErrorMessage()}",
                    "Make sure the method name is unique, in other words doesn't have overloads",
                    e);
            }

            string GetMethodNameForErrorMessage()
                => $"\"{MethodName}\"" + (MethodName == DefaultMethodName ? " (default)" : "");
        }
    }
}