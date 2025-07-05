using FluentAssertions;
using QKP.EzId.SourceGenerator.Tests.Helpers;
using Xunit;

namespace QKP.EzId.SourceGenerator.Tests
{
    public class EzIdTypeGeneratorTests
    {
        [Fact]
        public void Given_basic_id_type_when_generator_runs_then_should_generate_files()
        {
            var source = """
                         using QKP.EzId;

                         namespace TestNamespace
                         {
                             [EzIdType]
                             public readonly partial struct TestId
                             {
                             }
                         }
                         """;

            // Act
            var result = SourceGeneratorTestHelper.RunGenerator(source);

            // Assert
            var generatedFiles = result.GetGeneratedFilePaths().ToList();
            generatedFiles.Should().HaveCount(3);
            generatedFiles.Should().Contain("TestId.g.cs");
            generatedFiles.Should().Contain("TestIdJsonConverter.g.cs");
            generatedFiles.Should().Contain("TestIdNewtonsoftJsonConverter.g.cs");

            var idTypeContent = result.GetGeneratedOutput("TestId.g.cs");
            idTypeContent.Should().Contain("namespace TestNamespace");
            idTypeContent.Should().Contain("public readonly partial struct TestId :");
            idTypeContent.Should().Contain("private const char Separator = '-'");
            idTypeContent.Should().Contain("private static readonly int[] s_separatorPositions");
            idTypeContent.Should().Contain("private static readonly int s_length");
            idTypeContent.Should().Contain("public string Value { get; }");
            idTypeContent.Should().Contain("public static readonly TestId ErrorId");
            idTypeContent.Should().Contain("public static TestId Parse(");
            idTypeContent.Should().Contain("public static bool TryParse(");
        }

        [Fact]
        public void Given_id_type_with_custom_separator_when_generator_runs_then_should_generate_files_with_custom_separator()
        {
            var source = """

                         using QKP.EzId;

                         namespace TestNamespace
                         {
                             [EzIdType(Separator = SeparatorOptions.Underscore)]
                             public readonly partial struct TestSeparatorId
                             {
                             }
                         }
                         """;

            // Act
            var result = SourceGeneratorTestHelper.RunGenerator(source);

            // Assert
            var idTypeContent = result.GetGeneratedOutput("TestSeparatorId.g.cs");
            idTypeContent.Should().Contain("private const char Separator = '_'");
            idTypeContent.Should().NotContain("private const char Separator = '-'");
        }

        [Fact]
        public void Given_id_type_when_generator_runs_then_should_generate_json_converters()
        {
            var source = """

                         using QKP.EzId;

                         namespace TestNamespace
                         {
                             [EzIdType]
                             public partial struct TestJsonId
                             {
                             }
                         }
                         """;

            // Act
            var result = SourceGeneratorTestHelper.RunGenerator(source);

            // Assert
            var systemJsonContent = result.GetGeneratedOutput("TestJsonIdJsonConverter.g.cs");
            systemJsonContent.Should().Contain("namespace TestNamespace.Json");
            systemJsonContent.Should().Contain("public class TestJsonIdJsonConverter : JsonConverter<TestNamespace.TestJsonId>");
            var newtonsoftJsonContent = result.GetGeneratedOutput("TestJsonIdNewtonsoftJsonConverter.g.cs");
            newtonsoftJsonContent.Should().Contain("namespace TestNamespace.Json");
            newtonsoftJsonContent.Should().Contain("public class TestJsonIdNewtonsoftJsonConverter : JsonConverter<TestNamespace.TestJsonId>");
        }

        [Fact]
        public void Given_multiple_id_types_when_generator_runs_then_should_handle_multiple_id_types_in_same_compilation()
        {
            var source = """

                         using QKP.EzId;

                         namespace TestNamespace
                         {
                             [EzIdType]
                             public partial struct TypeAId
                             {
                             }

                             [EzIdType]
                             public partial struct TypeBId
                             {
                             }
                         }
                         """;
            // Act
            var result = SourceGeneratorTestHelper.RunGenerator(source);

            // Assert
            var generatedFiles = result.GetGeneratedFilePaths().ToList();
            generatedFiles.Should().HaveCount(6);
            generatedFiles.Should().Contain("TypeAId.g.cs");
            generatedFiles.Should().Contain("TypeBId.g.cs");
        }

        [Fact]
        public void Given_mixed_structs_when_generator_runs_then_should_ignore_structs_without_attribute()
        {
            var source = """

                         using QKP.EzId;

                         namespace TestNamespace
                         {
                             [EzIdType]
                             public partial struct MarkedId
                             {
                             }

                             public partial struct UnmarkedId
                             {
                             }
                         }
                         """;
            // Act
            var result = SourceGeneratorTestHelper.RunGenerator(source);
            // Assert
            var generatedFiles = result.GetGeneratedFilePaths().ToList();
            generatedFiles.Should().HaveCount(3);
            generatedFiles.Should().Contain("MarkedId.g.cs");
            generatedFiles.Should().NotContain("UnmarkedId.g.cs");
        }
    }
}
