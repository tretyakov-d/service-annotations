using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static ServiceAnnotations.Tests.AssemblyMaker;

namespace ServiceAnnotations.Tests
{
    public class MixedAttributeFacts
    {
        [Fact]
        public void Service_ConfigureServices_InvokedInTheOrderTheyWereApplied()
        {
            Assembly assembly = MakeAssembly(@"
                [Service(ServiceLifetime.Transient), ConfigureServices]
                class AnnotatedClass {
                    static void ConfigureServices(IServiceCollection svc) {
                        svc.AddTransient<AnnotatedClass, NotAnnotatedClass>() ;
                    }
                }
            
                class NotAnnotatedClass : AnnotatedClass { }
            ");

            Type annotated = assembly.GetType("AnnotatedClass");
            Type notAnnotated = assembly.GetType("NotAnnotatedClass");

            ServiceCollection sc = new ServiceCollection().AddAnnotatedServices(assembly);

            sc.Should().HaveCount(2, "1 registered by ServiceAttribute, 1 registered by ConfigureServices");
            
            sc.First().ImplementationType.Should().Be(annotated,
                "ServiceAttribute was placed first");

            sc.Last().ImplementationType.Should().Be(notAnnotated, 
                "ConfigureServicesAttribute was placed last");
        }
        
        [Fact]
        public void ConfigureServices_Service_InvokedInTheOrderTheyWereApplied()
        {
            Assembly assembly = MakeAssembly(@"
                [ConfigureServices, Service(ServiceLifetime.Transient)]
                class AnnotatedClass {
                    static void ConfigureServices(IServiceCollection svc) {
                        svc.AddTransient<AnnotatedClass, NotAnnotatedClass>() ;
                    }
                }
            
                class NotAnnotatedClass : AnnotatedClass { }
            ");

            Type annotated = assembly.GetType("AnnotatedClass");
            Type notAnnotated = assembly.GetType("NotAnnotatedClass");

            ServiceCollection sc = new ServiceCollection().AddAnnotatedServices(assembly);

            sc.Should().HaveCount(2, "1 registered by ServiceAttribute, 1 registered by ConfigureServices");
            
            sc.First().ImplementationType.Should().Be(notAnnotated, 
                "ConfigureServicesAttribute was placed first");
            
            sc.Last().ImplementationType.Should().Be(annotated,
                "ServiceAttribute was placed last");
        }
        
        [Fact]
        public void Service_CanUseConfigureServices_ToConfigureDependencies()
        {
            Assembly assembly = MakeAssembly(@"
                [Service(ServiceLifetime.Transient), ConfigureServices(""RegisterDependencies"")]
                class ServiceClass {
                    readonly HttpClient _httpClient;

                    public string Endpoint => _httpClient.BaseAddress.ToString();

                    public ServiceClass(HttpClient httpClient) => _httpClient = httpClient;

                    static void RegisterDependencies(IServiceCollection serviceCollection) {
                        serviceCollection.AddHttpClient<ServiceClass>(httpClient => {
                            httpClient.BaseAddress = new Uri(""https://github.com/"");
                        });
                    }
                }
            ");
            
            Type serviceType = assembly.GetType("ServiceClass");
            PropertyInfo endpointProperty = serviceType.GetProperty("Endpoint");

            var serviceInstance = new ServiceCollection()
                .AddAnnotatedServices(assembly)
                .BuildServiceProvider()
                .GetService(serviceType);

            var endpoint = endpointProperty.GetValue(serviceInstance, null);

            endpoint.Should().Be("https://github.com/");
        }
    }
}
