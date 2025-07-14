using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace QKP.EzId.SourceGenerator
{
    [Generator]
    internal class EzIdIncrementalSourceGenerator : IIncrementalGenerator
    {
        /// <summary>
        /// Initializes the generator.
        /// </summary>
        /// <param name="context">The generator initialization context.</param>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValuesProvider<StructDeclarationSyntax> structDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                    transform: static (ctx, _) =>
                        GetSemanticTargetForGeneration(ctx))
                .Where(static m => m is not null)!;

            IncrementalValueProvider<(Compilation, ImmutableArray<StructDeclarationSyntax>)> compilationAndEnums
                = context.CompilationProvider.Combine(structDeclarations.Collect());

            context.RegisterSourceOutput(compilationAndEnums,
                static (spc, source) => Execute(source.Item1, source.Item2, spc));
        }

        private static StructDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext ctx)
        {
            var structDeclarationSyntax = (StructDeclarationSyntax)ctx.Node;
            if (!structDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
            {
                return null;
            }

            foreach (AttributeListSyntax attributeListSyntax in structDeclarationSyntax.AttributeLists)
            {
                foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
                {
                    if (ctx.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                    {
                        continue;
                    }

                    INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                    string fullName = attributeContainingTypeSymbol.ToDisplayString();

                    if (fullName == "QKP.EzId.EzIdTypeAttribute")
                    {
                        return structDeclarationSyntax;
                    }
                }
            }

            return null;
        }

        private static EzIdTypeToGenerate? GetEzIdToGenerate(Compilation compilation, INamedTypeSymbol structSymbol)
        {
            INamedTypeSymbol? enumAttribute = compilation.GetTypeByMetadataName("QKP.EzId.EzIdTypeAttribute");

            if (enumAttribute == null)
            {
                return null;
            }

            foreach (AttributeData attributeData in structSymbol.GetAttributes())
            {
                if (!enumAttribute.Equals(attributeData.AttributeClass, SymbolEqualityComparer.Default))
                {
                    continue;
                }

                (int bitSize, int separatorEnumVal, int[]? separatorPositions) =
                    ExtractAttributeProperties(attributeData);
                int length = (int)Math.Ceiling((double)bitSize / Constants.Base32BitsPerChar);
                return new EzIdTypeToGenerate(bitSize, structSymbol.ContainingNamespace.ToDisplayString(),
                    structSymbol.Name, separatorEnumVal.Stringify(), separatorPositions.Stringify(),
                    (length + separatorPositions.Length).ToString());
            }

            return null;
        }

        private static (int bitSize, int separatorEnumVal, int[] separatorPositions) ExtractAttributeProperties(
            AttributeData attribute)
        {
            // Initialize with default values
            int bitSizeEnum = Constants.BitSize.Bits96;
            int separatorEnumVal = 1;
            int[] separatorPositions = [5, 15];

            // Process constructor arguments
            if (attribute.ConstructorArguments.Length > 0 && !attribute.ConstructorArguments[0].IsNull)
            {
                object? bitSizeValue = attribute.ConstructorArguments[0].Value;
                if (bitSizeValue is not null)
                {
                    bitSizeEnum = (int)bitSizeValue;
                }
            }

            if (attribute.ConstructorArguments.Length > 1 && !attribute.ConstructorArguments[1].IsNull)
            {
                object? separatorValue = attribute.ConstructorArguments[1].Value;
                if (separatorValue is not null)
                {
                    separatorEnumVal = (int)separatorValue;
                }
            }

            if (attribute.ConstructorArguments.Length > 2 && !attribute.ConstructorArguments[2].IsNull)
            {
                var typedArray = attribute.ConstructorArguments[2];
                separatorPositions = typedArray.Values
                    .Where(v => !v.IsNull)
                    .Select(v => (int)v.Value!)
                    .ToArray();
            }

            foreach (var namedArg in attribute.NamedArguments)
            {
                switch (namedArg.Key)
                {
                    case "BitSize" when !namedArg.Value.IsNull:
                        {
                            object? bitSizeValue = namedArg.Value.Value;
                            if (bitSizeValue is not null)
                            {
                                bitSizeEnum = (int)bitSizeValue;
                            }

                            break;
                        }
                    case "Separator" when !namedArg.Value.IsNull:
                        {
                            object? separatorValue = namedArg.Value.Value;
                            if (separatorValue is not null)
                            {
                                separatorEnumVal = (int)separatorValue;
                            }

                            break;
                        }
                    case "SeparatorPositions" when !namedArg.Value.IsNull:
                        {
                            var typedArray = namedArg.Value;
                            separatorPositions = typedArray.Values
                                .Where(v => !v.IsNull)
                                .Select(v => (int)v.Value!)
                                .ToArray();
                            break;
                        }
                }
            }

            if (separatorEnumVal == Constants.SeparatorEnumValues.None && separatorPositions.Length > 0)
            {
                throw new ArgumentException("Invalid separator positions for separator: none, pass in an empty int[].",
                    nameof(separatorPositions));
            }

            if (separatorEnumVal != Constants.SeparatorEnumValues.None)
            {
                switch (bitSizeEnum)
                {
                    case 96 when !separatorPositions.All(x => x is >= 0 and <= 19):
                        throw new ArgumentException(
                            $"Invalid separator positions for bitSize {bitSizeEnum} and separator {separatorEnumVal.Stringify()}, separator positions must be a number between 0 and 19",
                            nameof(separatorPositions));
                    case 64 when !separatorPositions.All(x => x is >= 0 and <= 12):
                        throw new ArgumentException(
                            $"Invalid separator positions for bitSize {bitSizeEnum} and separator {separatorEnumVal.Stringify()}, separator positions must be a number between 0 and 12",
                            nameof(separatorPositions));
                }
            }

            return (bitSizeEnum, separatorEnumVal, separatorPositions);
        }

        private static bool IsSyntaxTargetForGeneration(SyntaxNode syntaxNode) => syntaxNode is StructDeclarationSyntax
        {
            AttributeLists.Count: > 0
        };

        private static void Execute(Compilation compilation, ImmutableArray<StructDeclarationSyntax> structs,
            SourceProductionContext context)
        {
            if (structs.IsDefaultOrEmpty)
            {
                return;
            }

            foreach (var structDeclarationSyntax in structs.Distinct())
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    break;
                }

                SemanticModel semanticModel = compilation.GetSemanticModel(structDeclarationSyntax.SyntaxTree);
                if (semanticModel.GetDeclaredSymbol(structDeclarationSyntax) is not { } structSymbol)
                {
                    continue;
                }

                try
                {
                    var ezIdToGenerate = GetEzIdToGenerate(compilation, structSymbol);
                    if (ezIdToGenerate is null)
                    {
                        continue;
                    }

                    string template = ezIdToGenerate.BitSize switch
                    {
                        96 => nameof(Templates.EzIdImplementationTemplate),
                        64 => nameof(Templates.CompactEzIdTypeImplementationTemplate),
                        _ => throw new InvalidOperationException(
                            $"No template exists for bitSize: {ezIdToGenerate.BitSize}")
                    };

                    var replacements = new Dictionary<string, string>
                    {
                        ["Namespace"] = ezIdToGenerate.Namespace,
                        ["TypeName"] = ezIdToGenerate.TypeName,
                        ["Separator"] = ezIdToGenerate.Separator,
                        ["SeparatorPositions"] = ezIdToGenerate.SeparatorPositions,
                        ["Length"] = ezIdToGenerate.Length
                    };

                    var idTypeTemplate = TemplateProcessor.LoadTemplate(template);
                    string idTypeSource = idTypeTemplate.Process(replacements);
                    context.AddSource($"{ezIdToGenerate.TypeName}.g.cs", SourceText.From(idTypeSource, Encoding.UTF8));

                    var jsonConverterTemplate =
                        TemplateProcessor.LoadTemplate(nameof(Templates.JsonConverterImplementationTemplate));
                    string jsonConverterSource = jsonConverterTemplate.Process(replacements);
                    context.AddSource($"{ezIdToGenerate.TypeName}JsonConverter.g.cs",
                        SourceText.From(jsonConverterSource, Encoding.UTF8));
                }
                catch (Exception ex)
                {
                    // Report any errors that occur during template processing
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "EZID001",
                            "Error processing EzId type",
                            $"Error processing EzId type {structSymbol.Name}: {ex.Message}",
                            "EzId",
                            DiagnosticSeverity.Error,
                            isEnabledByDefault: true),
                        Location.Create(structDeclarationSyntax.SyntaxTree, structDeclarationSyntax.Span)));
                }
            }
        }
    }

    internal static class StringifyExtensions
    {
        public static string Stringify(this int separatorEnumVal)
        {
            string separator = separatorEnumVal switch
            {
                1 => "-",
                2 => "_",
                _ => "\0"
            };

            return separator;
        }

        public static string Stringify(this int[] array)
        {
            if (array.Length == 0)
            {
                return "Array.Empty<int>();";
            }

            var sb = new StringBuilder();
            sb.Append("new [] ");
            sb.Append("{ ");
            for (int i = 0; i < array.Length; i++)
            {
                sb.Append(array[i]);
                if (i != array.Length - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(" }");
            sb.Append(";");

            return sb.ToString();
        }
    }
}
