using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceAnnotations.Tests
{
    public static class AssemblyMaker
    {
        public static  Assembly MakeAssembly(string code = "")
            => MakeAssembly(CSharpCompilation.Create(
                "TEST",
                new[]
                {
                    SyntaxFactory.ParseSyntaxTree($@"
                        using ServiceAnnotations; 
                        using System;
                        using System.Net.Http;
                        using Microsoft.Extensions.DependencyInjection; 
                        using Microsoft.Extensions.Configuration;
                        {code}",
                        new CSharpParseOptions(LanguageVersion.Latest))
                },
                new[]
                {
                    GetAssemblyReference(typeof(string).Assembly),
                    GetAssemblyReference(typeof(ServiceAttribute).Assembly),
                    GetAssemblyReference(typeof(ServiceLifetime).Assembly),
                    GetAssemblyReference(typeof(IServiceProvider).Assembly),
                    GetAssemblyReference(typeof(IConfigurationRoot).Assembly),
                    GetAssemblyReference(typeof(Uri).Assembly),
                    GetAssemblyReference(typeof(HttpClient).Assembly),
                    GetAssemblyReference(typeof(HttpClientFactoryExtensions).Assembly),
                    GetAssemblyReference(Assembly.Load("netstandard, Version=2.0.0.0")),
                    GetAssemblyReference(Assembly.Load("System.Runtime, Version=5.0.0.0"))
                },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)));

        static Assembly MakeAssembly(Compilation compilation)
        {
            using var s = new MemoryStream();
            EmitResult result = compilation.Emit(s);

            if (!result.Success)
                throw new CompilationErrorException(
                    result.Diagnostics.Where(p => p.Severity == DiagnosticSeverity.Error)
                        .Select(p => p.GetMessage())
                        .FirstOrDefault(),
                    
                    result.Diagnostics);

            byte[] bytes = s.ToArray();
            return Assembly.Load(bytes);
        }

        static MetadataReference GetAssemblyReference(Assembly assembly)
            => AssemblyMetadata.CreateFromFile(assembly.Location).GetReference();
    }
}