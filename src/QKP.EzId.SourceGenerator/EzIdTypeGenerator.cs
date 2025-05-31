using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace QKP.EzId.SourceGenerator
{
    /// <summary>
    /// Source generator for EzId types.
    /// </summary>
    [Generator]
    public class EzIdTypeGenerator : ISourceGenerator
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

        private void ProcessStruct(GeneratorExecutionContext context, StructDeclarationSyntax structDeclaration)
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

            // Extract attribute properties
            (string separator, int[]? separatorPositions) = ExtractAttributeProperties(attribute);

            // Calculate the length of the ID
            int length = 13; // Default length for EzId (without separators)

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

            try
            {
                // Process the ID type implementation template
                var idTypeTemplate = TemplateProcessor.LoadTemplate("IdTypeImplementation.cs.template");
                string idTypeSource = idTypeTemplate.Process(replacements);
                context.AddSource($"{structSymbol!.Name}.g.cs", SourceText.From(idTypeSource, Encoding.UTF8));

                // Process the JSON converter template
                var jsonConverterTemplate = TemplateProcessor.LoadTemplate("JsonConverterImplementation.cs.template");
                string jsonConverterSource = jsonConverterTemplate.Process(replacements);
                context.AddSource($"{structSymbol.Name}JsonConverter.g.cs", SourceText.From(jsonConverterSource, Encoding.UTF8));

                // Process the Newtonsoft JSON converter template
                var newtonsoftJsonConverterTemplate = TemplateProcessor.LoadTemplate("NewtonsoftJsonConverterImplementation.cs.template");
                string newtonsoftJsonConverterSource = newtonsoftJsonConverterTemplate.Process(replacements);
                context.AddSource($"{structSymbol.Name}NewtonsoftJsonConverter.g.cs", SourceText.From(newtonsoftJsonConverterSource, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                // Report any errors that occur during template processing
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "EZID001",
                        "Error processing EzId type",
                        $"Error processing EzId type {structSymbol!.Name}: {ex.Message}",
                        "EzId",
                        DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    Location.Create(structDeclaration.SyntaxTree, structDeclaration.Span)));
            }
        }

        /// <summary>
        /// Extracts the properties from the EzIdTypeAttribute.
        /// </summary>
        /// <param name="attribute">The attribute data.</param>
        /// <returns>A tuple containing the separator and separatorPositions.</returns>
        private static (string separator, int[] separatorPositions) ExtractAttributeProperties(AttributeData attribute)
        {
            // Initialize with default values
            string separator = "-";
            int[] separatorPositions = [3, 10];

            // Process constructor arguments
            if (attribute.ConstructorArguments.Length > 0 && !attribute.ConstructorArguments[0].IsNull)
            {
                object? separatorValue = attribute.ConstructorArguments[0].Value;
                if (separatorValue is not null)
                {
                    int separatorEnumVal = (int)separatorValue;
                    separator = separatorEnumVal.Stringify();
                }
            }

            if (attribute.ConstructorArguments.Length > 1 && !attribute.ConstructorArguments[1].IsNull)
            {
                var typedArray = attribute.ConstructorArguments[1];
                separatorPositions = typedArray.Values
                    .Where(v => !v.IsNull)
                    .Select(v => (int)v.Value!)
                    .ToArray();
            }

            // Process named arguments (these override constructor arguments)
            foreach (var namedArg in attribute.NamedArguments)
            {
                if (namedArg.Key == "Separator" && !namedArg.Value.IsNull)
                {
                    object? separatorValue = namedArg.Value.Value;
                    if (separatorValue is not null)
                    {
                        int separatorEnumVal = (int)separatorValue;
                        separator = separatorEnumVal.Stringify();
                    }
                }
                else if (namedArg.Key == "SeparatorPositions" && !namedArg.Value.IsNull)
                {
                    var typedArray = namedArg.Value;
                    separatorPositions = typedArray.Values
                        .Where(v => !v.IsNull)
                        .Select(v => (int)v.Value!)
                        .ToArray();}
            }

            return (separator, separatorPositions);
        }

        /// <summary>
        /// Syntax receiver that finds struct declarations with the EzIdType attribute.
        /// </summary>
        private class SyntaxReceiver : ISyntaxContextReceiver
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

    public static class StringifyExtensions
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

            //new[] { 3, 10 };
            var sb = new StringBuilder();
            sb.Append("new [] ");
            sb.Append("{");
            for (int i = 0; i < array.Length; i++)
            {
                sb.Append(array[i]);
                if (i != array.Length - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append("}");
            sb.Append(";");

            return sb.ToString();
        }
    }
}
