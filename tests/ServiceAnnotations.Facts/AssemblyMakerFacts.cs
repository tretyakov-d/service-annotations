using System;
using System.Reflection;
using FluentAssertions;
using Microsoft.CodeAnalysis.Scripting;
using Xunit;
using static ServiceAnnotations.Tests.AssemblyMaker;

namespace ServiceAnnotations.Tests
{
    public class AssemblyMakerTests
    {
        [Fact]
        public void CanMakeAnAssembly()
        {
            Assembly assembly = MakeAssembly("public class MyClass {}");

            Type type = assembly.GetType("MyClass");
            type.Should().NotBeNull();
        }
        
        [Fact]
        public void WhenCompilationFails_ThrowsCompilationErrorException()
        {
            CompilationErrorException ex = Assert.Throws<CompilationErrorException>(() => MakeAssembly("public string Foo {get;set;}"));
            ex.Message.Should().Be("A namespace cannot directly contain members such as fields or methods");
            ex.Diagnostics.Should().NotBeNullOrEmpty();
        }
    }
}