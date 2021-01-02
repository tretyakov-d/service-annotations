using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static ServiceAnnotations.Tests.AssemblyMaker;

namespace ServiceAnnotations.Tests
{
    [Service(ServiceLifetime.Transient)]
    public class AnnotatedTestClass { }

    public class ServiceAnnotationUtilityTests
    {
        [Fact]
        public void AddAnnotatedServices_GiveAssembly_ReturnsSameServiceCollectionInstance()
        {
            ServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddAnnotatedServices(MakeAssembly())
                .Should().BeSameAs(serviceCollection);
        }

        [Fact]
        public void AddAnnotatedServices_GiveNoParams_ReturnsSameServiceCollectionInstance()
        {
            ServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddAnnotatedServices()
                .Should().BeSameAs(serviceCollection);
        }

        [Fact]
        public void GiveNoAssembly_RegistersCallingAssembly()
        {
            ServiceCollection serviceCollection = new ServiceCollection().AddAnnotatedServices();

            serviceCollection.Should().HaveCount(1,
                "For test purposes the ServiceAnnotations.Tests (the calling assembly) has only 1 annotated class," +
                $" and it is being {typeof(AnnotatedTestClass).FullName}");

            serviceCollection.Should().Contain(p => p.ServiceType == typeof(AnnotatedTestClass));
        }
    }
}