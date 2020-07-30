using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Grynwald.Utilities.Collections;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Xunit.Sdk;

namespace Grynwald.ChangeLog.Test
{
    /// <summary>
    /// Helper class providing for dynamically compiling C# code to an assembly using Roslyn.
    /// </summary>
    public static class CSharpCompiler
    {
        private const string s_DefaultAssemblyName = "DynamicTestAssembly";

        private static readonly Lazy<IReadOnlyList<MetadataReference>> s_DefaultMetadataReferences = new Lazy<IReadOnlyList<MetadataReference>>(() =>
        {
            var paths = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !x.IsDynamic)
                .Select(x => x.Location)
                .Where(File.Exists);

            return paths.Select(p => MetadataReference.CreateFromFile(p)).ToArray();
        });


        public static Assembly Compile(string sourceCode, string assemblyName = s_DefaultAssemblyName)
        {
            var compilation = GetCompilation(sourceCode, assemblyName);

            using var assemblyStream = new MemoryStream();

            var emitResult = compilation.Emit(peStream: assemblyStream);
            EnsureCompilationSucccess(emitResult);

            assemblyStream.Seek(0, SeekOrigin.Begin);


            return AssemblyLoadContext.Default.LoadFromStream(assemblyStream);
        }


        private static Compilation GetCompilation(string sourceCode, string assemblyName = s_DefaultAssemblyName)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

            var compilation = CSharpCompilation.Create(
              assemblyName,
              new[] { syntaxTree },
              GetMetadataReferences(),
              new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            return compilation;
        }

        private static IReadOnlyList<MetadataReference> GetMetadataReferences() => s_DefaultMetadataReferences.Value;

        private static void EnsureCompilationSucccess(EmitResult emitResult)
        {
            if (!emitResult.Success)
            {
                var errors = emitResult.Diagnostics
                 .Where(d => d.Severity >= DiagnosticSeverity.Error || d.IsWarningAsError)
                 .Select(d => d.GetMessage())
                .ToArray();

                throw new XunitException($"Failed to compile code to assembly:\r\n {errors.JoinToString("\r\n")}");
            }
        }
    }
}
