using System;
using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static ServiceAnnotations.Tests.AssemblyMaker;

namespace ServiceAnnotations.Tests
{
    public class ConfigureServicesAttributeFailFacts
    {
        [Fact]
        public void ConfigureServices_DefaultName_NotFound()
        {
            ShouldFail(@$"
                [ConfigureServices]
                public static class MyClass {{ }}
",
                "SA2000",
                @"Configure services method not found by name: ""ConfigureServices"" (default)",
                "Make sure the method exists and is static", exception => exception.InnerException.Should().BeNull());
        }

        [Fact]
        public void ConfigureServices_CustomName_NotFound()
        {
            ShouldFail(@$"
                [ConfigureServices(""CustomName"")]
                public static class MyClass {{ }}
",
                "SA2000",
                @"Configure services method not found by name: ""CustomName""",
                "Make sure the method exists and is static", exception => exception.InnerException.Should().BeNull());
        }

        [Fact]
        public void ConfigureServices_DefaultName_NotStatic_NotFound()
        {
            ShouldFail(@$"
                [ConfigureServices]
                public class MyClass {{
                    public void ConfigureServices(IServiceCollection serviceCollection) {{ }} 
 }}
",
                "SA2000",
                @"Configure services method not found by name: ""ConfigureServices"" (default)",
                "Make sure the method exists and is static", exception => exception.InnerException.Should().BeNull());
        }

        [Fact]
        public void ConfigureServices_CustomName_NotStatic_NotFound()
        {
            ShouldFail(@$"
                [ConfigureServices(""CustomName"")]
                public class MyClass {{
                    public void CustomName(IServiceCollection serviceCollection) {{ }} 
 }}
",
                "SA2000",
                @"Configure services method not found by name: ""CustomName""",
                "Make sure the method exists and is static", exception => exception.InnerException.Should().BeNull());
        }

        [Fact]
        public void ConfigureServices_AmbiguousMethodName_ConfigureServices_Throws_SA2001()
        {
            ShouldFail(@$"
                [ConfigureServices]
                public class MyClass {{
                    public static void ConfigureServices(IServiceCollection sc) {{ }}
                    public static void ConfigureServices(IServiceCollection sc, object whatever) {{ }}
                }}
",
                "SA2001",
                @"Ambiguous configure services method name: ""ConfigureServices"" (default)",
                "Make sure the method name is unique, in other words doesn't have overloads",
                exception => exception.InnerException.Should().BeOfType<AmbiguousMatchException>());
        }

        [Fact]
        public void ConfigureServices_AmbiguousMethodName_CustomName_Throws_SA2001()
        {
            ShouldFail(@$"
                [ConfigureServices(""CustomName"")]
                public class MyClass {{
                    public static void CustomName(IServiceCollection sc) {{ }}
                    public static void CustomName(IServiceCollection sc, object whatever) {{ }}
                }}
",
                "SA2001",
                @"Ambiguous configure services method name: ""CustomName""",
                "Make sure the method name is unique, in other words doesn't have overloads",
                exception => exception.InnerException.Should().BeOfType<AmbiguousMatchException>());
        }

        [Fact]
        public void ConfigureServices_AmbiguousMethodName_CustomName_Throws_SA2002()
        {
            ShouldFail(@$"
                namespace MyNamespace {{

                [ConfigureServices(""CustomName"")]
                public class MyClass {{
                    public static void CustomName(IServiceCollection sc, IConfiguration configuration) {{ }}
                }}
}}
",
                "SA2002",
                @"The instance of Microsoft.Extensions.Configuration.IConfiguration was not provided. " +
                "Required for parameter configuration in MyNamespace.MyClass.CustomName",
                "Make sure to add instance to annotation context. " +
                "For example serviceCollection.AddAnnotatedService(" +
                "context => context.Add<Microsoft.Extensions.Configuration.IConfiguration>(instance))",
                exception => exception.InnerException.Should().BeNull());
        }
       
        void ShouldFail(
            string assemblyCode,
            string expectedCode,
            string expectedDescription,
            string expectedTip,
            Action<ServiceAnnotationException> verify = null)
        {
            Assembly assembly = MakeAssembly(assemblyCode);

            var ex = Assert.Throws<ServiceAnnotationException>(
                () => new ServiceCollection().AddAnnotatedServices(assembly));

            ex.Should().NotBeNull();
            ex.Code.Should().Be(expectedCode);
            ex.Description.Should().Be(expectedDescription);
            ex.Tip.Should().Be(expectedTip);

            verify?.Invoke(ex);
        }
    }
}