using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static ServiceAnnotations.Tests.AssemblyMaker;

namespace ServiceAnnotations.Tests
{
    public class ServiceAttributeTests
    {
        [Theory, MemberData(nameof(ForAllLifetime))]
        public void GivenNoTypes_RegistersAsSelf(ServiceLifetime lifetime)
        {
            const string className = "Service";
            
            Assembly assembly = MakeAssembly(@$"
                [Service(ServiceLifetime.{lifetime})] 
                public class {className} {{ }}");
            
            Type classType = assembly.GetType(className)!;
            ServiceCollection serviceCollection = new ServiceCollection().AddAnnotatedServices(assembly);

            serviceCollection.Should().HaveCount(1);

            ServiceDescriptor serviceDescriptor = serviceCollection.Should()
                .Contain(p => p.ServiceType == classType)
                .Subject;

            serviceDescriptor.ImplementationType.Should().Be(classType);
            serviceDescriptor.Lifetime.Should().Be(lifetime);
        }
        
        [Theory, MemberData(nameof(ForAllLifetime))]
        public void GivenSingleType_RegistersAsGivenType(ServiceLifetime lifetime)
        {
            const string interfaceName = "IService";
            const string className = "Service";
        
            Assembly assembly = MakeAssembly(@$"
                interface {interfaceName} {{ }}
                [Service(ServiceLifetime.{lifetime}, typeof({interfaceName}))] 
                public class {className} : {interfaceName} {{ }}");

            Type classType = assembly.GetType(className);
            Type interfaceType = assembly.GetType(interfaceName);
            ServiceCollection serviceCollection = new ServiceCollection().AddAnnotatedServices(assembly);

            serviceCollection.Should().HaveCount(1);

            ServiceDescriptor serviceDescriptor = serviceCollection.Should()
                .HaveCount(1)
                .And.Contain(p => p.ServiceType == interfaceType)
                .Subject;

            serviceDescriptor.ImplementationType.Should().Be(classType);
            serviceDescriptor.Lifetime.Should().Be(lifetime);
        }

        [Theory, MemberData(nameof(ForAllLifetime))]
        public void GivenMultipleTypes_SelfFirst_RegistersAsSelf_MapsOthersToSelf(ServiceLifetime lifetime)
        {
            const string className = "Service";
            const string interfaceName = "IService";
            
            Assembly assembly = MakeAssembly($@"
                interface {interfaceName} {{ }}
                [Service(ServiceLifetime.{lifetime}, typeof({className}), typeof({interfaceName}))]
                class {className} : {interfaceName} {{ }}");

            Type interfaceType = assembly.GetType(interfaceName);
            Type classType = assembly.GetType(className);
            ServiceCollection serviceCollection = new ServiceCollection().AddAnnotatedServices(assembly);

            serviceCollection.Should().HaveCount(2, $"One for {className} one for {interfaceName}");

            ServiceDescriptor selfDescriptor = serviceCollection.Should()
                .Contain(p => p.ServiceType == classType)
                .Subject;

            selfDescriptor.ImplementationType.Should().Be(classType);
            selfDescriptor.Lifetime.Should().Be(lifetime);
            
            ShouldHaveValidMapping(serviceCollection, interfaceType, classType, lifetime);
        }

        [Theory, MemberData(nameof(ForAllLifetime))]
        public void GivenMultipleTypes_SelfLast_RegistersAsSelf_MapsOthersToSelf(ServiceLifetime lifetime)
        {
            const string className = "Service";
            const string interfaceName = "IService";
            
            Assembly assembly = MakeAssembly($@"
                interface {interfaceName} {{ }}
                [Service(ServiceLifetime.{lifetime}, typeof({interfaceName}), typeof({className}))]
                class {className} : {interfaceName} {{ }}");

            Type interfaceType = assembly.GetType(interfaceName);
            Type classType = assembly.GetType(className);
            ServiceCollection serviceCollection = new ServiceCollection().AddAnnotatedServices(assembly);
            
            serviceCollection.Should().HaveCount(2, $"One for {className} one for {interfaceName}");

            ServiceDescriptor selfDescriptor = serviceCollection.Should()
                .Contain(p => p.ServiceType == classType)
                .Subject;

            selfDescriptor.ImplementationType.Should().Be(classType);
            selfDescriptor.Lifetime.Should().Be(lifetime);
            
            ShouldHaveValidMapping(serviceCollection, interfaceType, classType, lifetime);
        }
        
        [Theory, MemberData(nameof(ForAllLifetime))]
        public void GivenMultipleTypes_NoSelf_RegistersFirst_MapsOthersToFirst(ServiceLifetime lifetime)
        {
            const string className = "Service";
            const string interfaceName = "IService";
            const string otherInterfaceName = "IOtherService";
            
            Assembly assembly = MakeAssembly($@"
                interface {interfaceName} {{ }}
                interface {otherInterfaceName} {{ }}
                [Service(ServiceLifetime.{lifetime}, typeof({interfaceName}), typeof({otherInterfaceName}))]
                class {className} : {interfaceName}, {otherInterfaceName} {{ }}");

            Type otherInterfaceType = assembly.GetType(otherInterfaceName);
            Type interfaceType = assembly.GetType(interfaceName);
            Type classType = assembly.GetType(className);
            ServiceCollection serviceCollection = new ServiceCollection().AddAnnotatedServices(assembly);

            ServiceDescriptor selfDescriptor = serviceCollection.Should()
                .Contain(p => p.ServiceType == interfaceType)
                .Subject;
            
            serviceCollection.Should().HaveCount(2, $"One for {interfaceName} one for {otherInterfaceName}");

            selfDescriptor.ImplementationType.Should().Be(classType);
            selfDescriptor.Lifetime.Should().Be(lifetime);
            
            ShouldHaveValidMapping(serviceCollection, otherInterfaceType, interfaceType, lifetime);
        }
        
        public static IEnumerable<object[]> ForAllLifetime() =>
            Enum.GetValues<ServiceLifetime>().Select(p => new object[] {p});
        
        private void ShouldHaveValidMapping(
            IServiceCollection serviceCollection, 
            Type fromType,
            Type toType, 
            ServiceLifetime lifetime)
        {
            ServiceDescriptor mappingDescriptor = serviceCollection.Should().Contain(
                sd => sd.ServiceType == fromType).Subject;

            mappingDescriptor.Lifetime.Should().Be(lifetime);
            mappingDescriptor.ImplementationFactory.Should().NotBeNull();

            ServiceProvider sp = serviceCollection.BuildServiceProvider();

            var selfInstance = sp.GetService(toType);
            var mappedInstance = sp.GetService(fromType);

            if (lifetime != ServiceLifetime.Transient)
                mappedInstance.Should().BeSameAs(selfInstance,
                    $"With {lifetime} lifetime mapping should return same instance");
        }
    }
}