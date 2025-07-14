using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace QKP.EzId.SourceGenerator.Tests.Helpers;

public class TestCompilation
{
    public static (Compilation, IIncrementalGenerator) CreateCompilation(string source)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

        // Create references for necessary assemblies
        List<MetadataReference> references = new();

        // Add reference to System.Runtime
        Assembly runtimeAssembly = Assembly.Load("System.Runtime");
        references.Add(MetadataReference.CreateFromFile(runtimeAssembly.Location));

        // Add reference to System.Private.CoreLib (for basic types)
        Assembly coreLibAssembly = typeof(object).Assembly;
        references.Add(MetadataReference.CreateFromFile(coreLibAssembly.Location));

        // Add a reference to the source generator assembly itself
        string sourceGeneratorAssemblyPath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
            "QKP.EzId.SourceGenerator.dll");

        string ezIdAssemblyPath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
            "QKP.EzId.dll");

        if (File.Exists(sourceGeneratorAssemblyPath))
        {
            references.Add(MetadataReference.CreateFromFile(sourceGeneratorAssemblyPath));
        }

        if (File.Exists(ezIdAssemblyPath))
        {
            references.Add(MetadataReference.CreateFromFile(ezIdAssemblyPath));
        }

        // Create the compilation
        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestCompilation",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Create the generator
        EzIdIncrementalSourceGenerator generator = new();

        return (compilation, generator);
    }
}
