using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static ServiceAnnotations.Tests.AssemblyMaker;

namespace ServiceAnnotations.Tests
{
    [Service(ServiceLifetime.Scoped)]
    public class AnnotatedTestClass { }

    public class ServiceAnnotationUtilityTests
    {
        [Fact]
        public void AddAnnotatedServices_GivenAssembly_ReturnsSameServiceCollectionInstance()
        {
            ServiceCollection serviceCollection = new();

            serviceCollection.AddAnnotatedServices(MakeAssembly())
                .Should().BeSameAs(serviceCollection);
        }

        [Fact]
        public void AddAnnotatedServices_GivenNoParams_ReturnsSameServiceCollectionInstance()
        {
            ServiceCollection serviceCollection = new();

            serviceCollection.AddAnnotatedServices()
                .Should().BeSameAs(serviceCollection);
        }

        [Fact]
        public void AddAnnotatedServices_GiveNoAssembly_RegistersCallingAssembly()
        {
            ServiceCollection serviceCollection = new ServiceCollection().AddAnnotatedServices();

            serviceCollection.Should().HaveCount(1,
                "For test purposes the ServiceAnnotations.Facts (the calling assembly) has only 1 annotated class," +
                $" and it is being {typeof(AnnotatedTestClass).FullName}");

            serviceCollection.Should().Contain(p => p.ServiceType == typeof(AnnotatedTestClass));
        }
        
        
        [Fact]
        public void AddAnnotatedServices_GiveNoAssembly_RegistersCallingAssembly_WithContext()
        {
            ServiceCollection serviceCollection = new ServiceCollection()
                .AddAnnotatedServices(context => context.Add(new object()));

            serviceCollection.Should().HaveCount(1,
                "For test purposes the ServiceAnnotations.Facts (the calling assembly) has only 1 annotated class," +
                $" and it is being {typeof(AnnotatedTestClass).FullName}");

            serviceCollection.Should().Contain(p => p.ServiceType == typeof(AnnotatedTestClass));
        }
    }
}