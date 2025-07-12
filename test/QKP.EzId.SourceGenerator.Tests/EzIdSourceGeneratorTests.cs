using FluentAssertions;
using QKP.EzId.SourceGenerator.Tests.Helpers;
using Xunit;

namespace QKP.EzId.SourceGenerator.Tests
{
    public class EzIdSourceGeneratorTests
    {
        [Fact]
        public void
            Given_id_type_with_empty_args_on_ez_id_type_attribute_when_generating_then_it_must_generate_with_default_values()
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
            generatedFiles.Should().HaveCount(2);
            generatedFiles.Should().Contain("TestId.g.cs");
            generatedFiles.Should().Contain("TestIdJsonConverter.g.cs");

            var idTypeContent = result.GetGeneratedOutput("TestId.g.cs");
            idTypeContent.Should().Contain("namespace TestNamespace");
            idTypeContent.Should().Contain("public readonly partial struct TestId :");
            idTypeContent.Should().Contain("private const char Separator = '-'");
            idTypeContent.Should().Contain("private static readonly int[] s_separatorPositions = new [] { 5, 15 };");
            idTypeContent.Should().Contain("private static readonly int s_length = 22;");
        }

        [Fact]
        public void
            Given_id_type_with_named_args_with_64_bits_and_underscore_separator_on_two_positions__when_generating_then_it_must_generate_expected_files()
        {
            var source = """
                         using QKP.EzId;

                         namespace TestNamespace
                         {
                             [EzIdType(separatorPositions: new int[] { 3, 10 }, separator: SeparatorOptions.Underscore, bitSize: IdBitSize.Bits64)]
                             public readonly partial struct TestId
                             {
                             }
                         }
                         """;

            // Act
            var result = SourceGeneratorTestHelper.RunGenerator(source);

            // Assert
            var generatedFiles = result.GetGeneratedFilePaths().ToList();
            generatedFiles.Should().HaveCount(2);
            generatedFiles.Should().Contain("TestId.g.cs");
            generatedFiles.Should().Contain("TestIdJsonConverter.g.cs");

            var idTypeContent = result.GetGeneratedOutput("TestId.g.cs");
            idTypeContent.Should().Contain("namespace TestNamespace");
            idTypeContent.Should().Contain(" [JsonConverter(typeof(TestIdJsonConverter))]");
            idTypeContent.Should().Contain("public readonly partial struct TestId :");
            idTypeContent.Should().Contain("ICompactEzId<TestId>");
            idTypeContent.Should().Contain("private const char Separator = '_'");
            idTypeContent.Should().Contain("private static readonly int[] s_separatorPositions = new [] { 3, 10 };");
            idTypeContent.Should().Contain("private static readonly int s_length = 15;");
        }

        [Fact]
        public void
            Given_id_type_with_none_separator_when_generating_then_it_should_generate_files_with_no_separator_and_empty_positions()
        {
            var source = """

                         using QKP.EzId;

                         namespace TestNamespace
                         {
                             [EzIdType(IdBitSize.Bits96, SeparatorOptions.None, new int[0])]
                             public readonly partial struct TestSeparatorId
                             {
                             }
                         }
                         """;

            // Act
            var result = SourceGeneratorTestHelper.RunGenerator(source);

            // Assert
            var idTypeContent = result.GetGeneratedOutput("TestSeparatorId.g.cs");
            idTypeContent.Should().Contain("namespace TestNamespace");
            idTypeContent.Should().Contain("[JsonConverter(typeof(TestSeparatorIdJsonConverter))]");
            idTypeContent.Should().Contain("public readonly partial struct TestSeparatorId :");
            idTypeContent.Should().Contain("private const char Separator = '\0';");
            idTypeContent.Should().Contain("private static readonly int[] s_separatorPositions = Array.Empty<int>();");
            idTypeContent.Should().Contain("private static readonly int s_length = 20;");
        }

        [Fact]
        public void Given_id_type_with_separator_positions_out_of_range_when_generating_then_it_should_throw_and_not_generate_files()
        {
            var source = """

                         using QKP.EzId;

                         namespace TestNamespace
                         {
                             [EzIdType(IdBitSize.Bits96, SeparatorOptions.Dash, new int[] { -1, 21 })]
                             public readonly partial struct TestSeparatorId
                             {
                             }
                         }
                         """;

            // Act
            var result = SourceGeneratorTestHelper.RunGenerator(source);

            // Assert
            var diagnostic = result.Diagnostics.Should().ContainSingle().Which;
            diagnostic.Id.Should().Be("EZID001");
            diagnostic.GetMessage().Should().Be("Error processing EzId type TestSeparatorId: Invalid separator positions for bitSize 96 and separator -, separator positions must be a number between 0 and 19 (Parameter 'separatorPositions')");
            result.GetGeneratedOutput("TestSeparatorId.g.cs").Should().BeEmpty();
    }

        [Fact]
        public void Given_id_type_with_no_separator_and_separator_positions_when_generating_then_it_should_throw_and_not_generate_files()
        {
            var source = """

                         using QKP.EzId;

                         namespace TestNamespace
                         {
                             [EzIdType(IdBitSize.Bits96, SeparatorOptions.None, new int[] { 5, 15 })]
                             public readonly partial struct TestSeparatorId
                             {
                             }
                         }
                         """;

            // Act
            var result = SourceGeneratorTestHelper.RunGenerator(source);

            // Assert
            var diagnostic = result.Diagnostics.Should().ContainSingle().Which;
            diagnostic.Id.Should().Be("EZID001");
            diagnostic.GetMessage().Should().Be("Error processing EzId type TestSeparatorId: Invalid separator positions for separator: none, pass in an empty int[]. (Parameter 'separatorPositions')");
            result.GetGeneratedOutput("TestSeparatorId.g.cs").Should().BeEmpty();
        }



    [Fact]
        public void Given_id_type_when_generating_then_it_should_generate_json_converter()
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
        }

        [Fact]
        public void Given_multiple_id_types_when_generating_then_it_should_handle_multiple_id_types_in_same_compilation()
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
            generatedFiles.Should().HaveCount(4);
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
            generatedFiles.Should().HaveCount(2);
            generatedFiles.Should().Contain("MarkedId.g.cs");
            generatedFiles.Should().NotContain("UnmarkedId.g.cs");
        }
    }
}
