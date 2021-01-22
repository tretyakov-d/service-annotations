using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static ServiceAnnotations.Tests.AssemblyMaker;

namespace ServiceAnnotations.Tests
{
    public class ConfigureServicesAttributeFacts
    {
        [Theory]
        [InlineData("public")]
        [InlineData("internal")]
        [InlineData("private")]
        public void NoNameGiven_Invokes_ConfigureServices_Method(string methodAccessModifier)
        {
            Assembly assembly = MakeAssembly(@$"
                class NotAnnotatedClass {{ }}

                [ConfigureServices]
                static class AnnotatedClass {{
                    {methodAccessModifier} static void ConfigureServices(IServiceCollection sc) {{
                        sc.AddScoped<NotAnnotatedClass>();
                    }}
                }}
");

            ServiceCollection serviceCollection = new ServiceCollection().AddAnnotatedServices(assembly);

            Type notAnnotatedType = assembly.GetType("NotAnnotatedClass");

            ServiceDescriptor serviceDescriptor = serviceCollection.Should().Contain(
                p => p.ServiceType == notAnnotatedType).Subject;

            serviceDescriptor.ImplementationType.Should().Be(notAnnotatedType);
            serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
        }

        [Fact]
        public void WithNameGiven_Invokes_SpecifiedMethod()
        {
            Assembly assembly = MakeAssembly(@$"
                class NotAnnotatedClass {{ }}

                [ConfigureServices(""NamedMethod"")]
                static class AnnotatedClass {{
                    static void NamedMethod(IServiceCollection sc) {{
                        sc.AddScoped<NotAnnotatedClass>();
                    }}
                }}
");

            ServiceCollection serviceCollection = new ServiceCollection().AddAnnotatedServices(assembly);

            Type notAnnotatedType = assembly.GetType("NotAnnotatedClass");

            ServiceDescriptor serviceDescriptor = serviceCollection.Should().Contain(
                p => p.ServiceType == notAnnotatedType).Subject;

            serviceDescriptor.ImplementationType.Should().Be(notAnnotatedType);
            serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
        }

        [Fact]
        public void WithContextServices_ResolvesSuccessfully()
        {
            Assembly assembly = MakeAssembly(@"
                class NotAnnotatedClass {
                    public string ConnectionString { get; set; }
                }

                [ConfigureServices]
                static class AnnotatedClass {
                    static void ConfigureServices(IServiceCollection sc, IConfiguration configuration) {
                        sc.AddTransient((sp) => 
                            new NotAnnotatedClass { 
                                ConnectionString = configuration.GetConnectionString(""test"") 
                            });
                    }
                }
");
            
            Type notAnnotatedType = assembly.GetType("NotAnnotatedClass");
            PropertyInfo connectionStringProperty = notAnnotatedType!.GetProperty("ConnectionString");

            IConfigurationRoot configuration = new ConfigurationBuilder().AddInMemoryCollection(
                    new[] {new KeyValuePair<string, string>("connectionStrings:test", "Hello World")})
                .Build();

            ServiceCollection serviceCollection = new ServiceCollection()
                .AddAnnotatedServices(assembly, 
                    context => context.Add<IConfiguration>(configuration));

            ServiceDescriptor serviceDescriptor = serviceCollection.Should().Contain(
                p => p.ServiceType == notAnnotatedType).Subject;

            serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Transient);
            serviceDescriptor.ImplementationFactory.Should().NotBeNull();

            object service = serviceCollection.BuildServiceProvider().GetService(notAnnotatedType);
            object value = connectionStringProperty!.GetValue(service, null);

            value.Should().Be("Hello World");
        }
        
        [Fact]
        public void NonStaticMethodsWithTheSameName_DoesntThrow()
        {
            Assembly assembly = MakeAssembly(@"
                [ConfigureServices]
                class AnnotatedClass {
                    static void ConfigureServices(IServiceCollection sc) { }
                    void ConfigureServices(IServiceCollection sc, bool whatever) { }
                }
");
            assembly.Should().NotBeNull();
        }
    }
}
