using FluentAssertions;
using Xunit;

namespace QKP.EzId.SourceGenerator.Tests
{
    public class TemplateProcessorTests
    {

        [Fact]
        public void Given_template_with_placeholders_when_processing_then_it_must_replace_placeholders()
        {
            var processor = TemplateProcessor.LoadTemplate("IdTypeImplementation.cs.template");
            var replacements = new Dictionary<string, string>
            {
                ["Namespace"] = "TestNamespace",
                ["TypeName"] = "TestId",
                ["Separator"] = "-",
                ["SeparatorPositions"] = "new[] { 4, 8 };",
                ["Length"] = "13"
            };

            // Act
            string result = processor.Process(replacements);

            // Assert
            result.Should().Contain("namespace TestNamespace");
            result.Should().Contain("public readonly partial struct TestId");
            result.Should().Contain("private const char Separator = '-';");
            result.Should().Contain("private static readonly int[] s_separatorPositions = new[] { 4, 8 };");
            result.Should().Contain("private const int s_length = 13;");
        }

        [Theory]
        [InlineData("IdTypeImplementation.cs.template")]
        [InlineData("JsonConverterImplementation.cs.template")]
        [InlineData("NewtonsoftJsonConverterImplementation.cs.template")]
        public void Given_valid_template_when_loading_template_then_it_must_load_processor(string template)
        {
            // Act
            var processor = TemplateProcessor.LoadTemplate(template);

            // Assert
            processor.Should().NotBeNull();
        }

        [Fact]
        public void Given_non_existent_template_when_loading_template_then_it_must_throw()
        {
            Func<TemplateProcessor> act = () => TemplateProcessor.LoadTemplate("NonExistentTemplate.cs.template");
            act.Should().Throw<InvalidOperationException>();
        }
    }
}
