using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace QKP.EzId.SourceGenerator
{
    internal static class Constants
    {
        public const int Base32BitsPerChar = 5;
        public static class BitSize
        {
            public const int Bits96 = 96;
            public const int Bits64 = 64;
        }

        public static class SeparatorEnumValues
        {
            public const int None = 0;
            public const int Dash = 1;
            public const int Underscore = 2;
        }
    }
    /// <summary>
    /// Source generator for EzId types.
    /// </summary>
    [Generator]
    public class EzIdSourceGenerator : ISourceGenerator
    {
        /// <summary>
        /// Initializes the generator.
        /// </summary>
        /// <param name="context">The generator initialization context.</param>
        public void Initialize(GeneratorInitializationContext context)
        {
            // Register a syntax receiver that will be created for each generation pass
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        /// <summary>
        /// Executes the generator.
        /// </summary>
        /// <param name="context">The generator execution context.</param>
        public void Execute(GeneratorExecutionContext context)
        {
            // Get the syntax receiver
            if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver))
                return;

            // Process each candidate struct
            foreach (var structDeclaration in receiver.CandidateStructs)
            {
                ProcessStruct(context, structDeclaration);
            }
        }

        private static void ProcessStruct(GeneratorExecutionContext context, StructDeclarationSyntax structDeclaration)
        {
            // Get the semantic model for the struct
            var semanticModel = context.Compilation.GetSemanticModel(structDeclaration.SyntaxTree);
            var structSymbol = semanticModel.GetDeclaredSymbol(structDeclaration);
            var ezIdTypeAttrSymbol = context.Compilation.GetTypeByMetadataName("QKP.EzId.EzIdTypeAttribute");
            if (structSymbol == null || ezIdTypeAttrSymbol == null)
                return;

            var attribute = structSymbol.GetAttributes().FirstOrDefault(a =>
                SymbolEqualityComparer.Default.Equals(a.AttributeClass, ezIdTypeAttrSymbol));
            if (attribute == null)
                return;

            try
            {
                // Extract attribute properties
                (int bitSize, string separator, int[]? separatorPositions) = ExtractAttributeProperties(attribute);

                // Calculate the length of the ID
                int length = (int)Math.Ceiling((double)bitSize / Constants.Base32BitsPerChar);

                var replacements = new Dictionary<string, string>
                {
                    ["Namespace"] = structSymbol.ContainingNamespace.ToDisplayString(),
                    ["TypeName"] = structSymbol.Name,
                    ["Separator"] = separator,
                    ["SeparatorPositions"] = separatorPositions.Stringify(),
                    ["Length"] = (length + separatorPositions.Length).ToString()
                };

                // Debug the replacements
                foreach (var replacement in replacements)
                {
                    Debug.WriteLine($"Replacement: {replacement.Key} = {replacement.Value}");
                }

                // Process the ID type implementation template
                string template = bitSize switch
                {
                    96 => nameof(Templates.EzIdImplementationTemplate),
                    64 => nameof(Templates.CompactEzIdTypeImplementationTemplate),
                    _ => throw new InvalidOperationException($"No template exists for bitSize: {bitSize}")
                };

                var idTypeTemplate = TemplateProcessor.LoadTemplate(template);
                string idTypeSource = idTypeTemplate.Process(replacements);
                context.AddSource($"{structSymbol.Name}.g.cs", SourceText.From(idTypeSource, Encoding.UTF8));

                // Process the JSON converter template
                var jsonConverterTemplate = TemplateProcessor.LoadTemplate(nameof(Templates.JsonConverterImplementationTemplate));
                string jsonConverterSource = jsonConverterTemplate.Process(replacements);
                context.AddSource($"{structSymbol.Name}JsonConverter.g.cs", SourceText.From(jsonConverterSource, Encoding.UTF8));
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
                    Location.Create(structDeclaration.SyntaxTree, structDeclaration.Span)));
            }
        }

        private static (int bitSize, string separator, int[] separatorPositions) ExtractAttributeProperties(AttributeData attribute)
        {
            // Initialize with default values
            int bitSizeEnum = Constants.BitSize.Bits96;
            int separatorEnumVal = 1;
            int[] separatorPositions = [5,15];

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

            // Process named arguments (these override constructor arguments)
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
                throw new ArgumentException("Invalid separator positions for separator: none, pass in an empty int[].", nameof(separatorPositions));
            }

            if(separatorEnumVal != Constants.SeparatorEnumValues.None)
            {
                switch (bitSizeEnum)
                {
                    case 96 when !separatorPositions.All(x => x is >= 0 and <= 19):
                        throw new ArgumentException($"Invalid separator positions for bitSize {bitSizeEnum} and separator {separatorEnumVal.Stringify()}, separator positions must be a number between 0 and 19", nameof(separatorPositions));
                    case 64 when !separatorPositions.All(x => x is >= 0 and <= 12):
                        throw new ArgumentException($"Invalid separator positions for bitSize {bitSizeEnum} and separator {separatorEnumVal.Stringify()}, separator positions must be a number between 0 and 12", nameof(separatorPositions));
                }
            }

            return (bitSizeEnum, separatorEnumVal.Stringify(), separatorPositions);
        }

        /// <summary>
        /// Syntax receiver that finds struct declarations with the EzIdType attribute.
        /// </summary>
        private sealed class SyntaxReceiver : ISyntaxContextReceiver
        {
            public List<StructDeclarationSyntax> CandidateStructs { get; } = [];

            /// <summary>
            /// Called for each node in the syntax tree to identify candidate nodes for processing.
            /// </summary>
            /// <param name="context">The generator syntax context.</param>
            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                // Check if the node is a struct declaration
                if (context.Node is StructDeclarationSyntax structDeclaration && structDeclaration.AttributeLists.Count > 0)
                {
                    // Check if the struct is partial
                    bool isPartial = structDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
                    if (!isPartial)
                    {
                        // Skip non-partial structs
                        return;
                    }

                    // Check if the struct has the EzIdType attribute
                    var semanticModel = context.SemanticModel;
                    var structSymbol = semanticModel.GetDeclaredSymbol(structDeclaration);
                    var ezIdTypeAttrSymbol = semanticModel.Compilation.GetTypeByMetadataName("QKP.EzId.EzIdTypeAttribute");

                    if (structSymbol != null && ezIdTypeAttrSymbol != null && structSymbol.GetAttributes().Any(a =>
                        SymbolEqualityComparer.Default.Equals(a.AttributeClass, ezIdTypeAttrSymbol)))
                    {
                        CandidateStructs.Add(structDeclaration);
                    }
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
