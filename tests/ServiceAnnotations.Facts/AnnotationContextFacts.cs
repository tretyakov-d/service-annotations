using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ServiceAnnotations.Tests
{
    public class AnnotationContextFacts
    {
        [Fact]
        public void TargetCollection_ReturnsSameInstanceAsPassedToBuilder()
        {
            var serviceCollection = new ServiceCollection();
            var builder = new AnnotationContext.Builder(serviceCollection);

            AnnotationContext context = builder.Build();

            context.TargetCollection.Should().BeSameAs(serviceCollection);
        }
        
        [Fact]
        public void GetService_IServiceCollection_ShouldBeAvailable()
        {
            var serviceCollection = new ServiceCollection();
            var builder = new AnnotationContext.Builder(serviceCollection);

            AnnotationContext context = builder.Build();

            context.GetService(typeof(IServiceCollection)).Should().BeSameAs(serviceCollection);
        }
        
        [Fact]
        public void GetService_ServiceCollection_ShouldBeAvailableByExplicitType()
        {
            var serviceCollection = new ServiceCollection();
            var builder = new AnnotationContext.Builder(serviceCollection);

            AnnotationContext context = builder.Build();

            context.GetService(typeof(ServiceCollection)).Should().BeSameAs(serviceCollection);
        }
        
        [Fact]
        public void GetService_Unknown_ShouldBeNull()
        {
            var serviceCollection = new ServiceCollection();
            var builder = new AnnotationContext.Builder(serviceCollection);

            AnnotationContext context = builder.Build();

            context.GetService(typeof(IEnumerable<string>)).Should().BeNull();
        }
        
        [Fact]
        public void GetService_AddedByExplicitType_ShouldBeAvailableByExplicitType()
        {
            var serviceCollection = new ServiceCollection();
            var builder = new AnnotationContext.Builder(serviceCollection);

            var service = new List<string>();
            
            builder.Add(service);
            
            AnnotationContext context = builder.Build();

            context.GetService(typeof(List<string>)).Should().BeSameAs(service);
        }
        
        [Fact]
        public void GetService_AddedByExplicitType_ByImplementedInterfaces_ShouldNoBeNull()
        {
            var serviceCollection = new ServiceCollection();
            var builder = new AnnotationContext.Builder(serviceCollection);

            builder.Add(new List<string>());
            
            AnnotationContext context = builder.Build();

            context.GetService(typeof(IEnumerable<string>)).Should().BeNull();
        }
        
        [Fact]
        public void GetService_AddedByInterface_ShouldBeAvailableByInterface()
        {
            var serviceCollection = new ServiceCollection();
            var builder = new AnnotationContext.Builder(serviceCollection);

            var service = new List<string>();
            
            builder.Add<IEnumerable<string>>(service);
            
            AnnotationContext context = builder.Build();

            context.GetService(typeof(IEnumerable<string>)).Should().BeSameAs(service);
        }
        
        [Fact]
        public void GetService_AddedByInterface_ByExplicitType_ShouldBuNull()
        {
            var serviceCollection = new ServiceCollection();
            var builder = new AnnotationContext.Builder(serviceCollection);

            builder.Add<IEnumerable<string>>(new List<string>());
            
            AnnotationContext context = builder.Build();

            context.GetService(typeof(List<string>)).Should().BeNull();
        }
        
        [Fact]
        public void GetService_SameInstance_ByVariousKeys_ShouldReturnSame()
        {
            var serviceCollection = new ServiceCollection();
            var builder = new AnnotationContext.Builder(serviceCollection);

            var service = new List<string>();
            
            builder.Add(service);
            builder.Add<ICollection<string>>(service);
            builder.Add<IEnumerable<string>>(service);
            
            AnnotationContext context = builder.Build();

            context.GetService(typeof(List<string>))
                .Should().BeSameAs(context.GetService(typeof(ICollection<string>)));
            
            context.GetService(typeof(ICollection<string>))
                .Should().BeSameAs(context.GetService(typeof(IEnumerable<string>)));
        }
        
        [Fact]
        public void Builder_Add_ReturnsInstanceOfBuilderForConvinientSequentialAdd()
        {
            var builder = new AnnotationContext.Builder(new ServiceCollection());
            var addResult = builder.Add<ICollection<string>>(new List<string>());

            builder.Should().BeSameAs(addResult);

        }
    }
}