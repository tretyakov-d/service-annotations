using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Scripting;
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
                        using Microsoft.Extensions.DependencyInjection; 
                        {code}",
                        new CSharpParseOptions(LanguageVersion.Latest))
                },
                new[]
                {
                    GetAssemblyReference(typeof(string).Assembly),
                    GetAssemblyReference(typeof(ServiceAttribute).Assembly),
                    GetAssemblyReference(typeof(ServiceLifetime).Assembly),
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
                    result.Diagnostics.Select(p => p.GetMessage()).FirstOrDefault(),
                    result.Diagnostics);

            byte[] bytes = s.ToArray();
            return Assembly.Load(bytes);
        }

        static MetadataReference GetAssemblyReference(Assembly assembly)
            => AssemblyMetadata.CreateFromFile(assembly.Location).GetReference();
    }
}