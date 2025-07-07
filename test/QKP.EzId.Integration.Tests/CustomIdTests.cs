using FluentAssertions;
using QKP.EzId.CustomTypes;
using Xunit;

namespace QKP.EzId.Integration.Tests;

public class CustomIdTests
{
    [Fact]
    public void Given_id_with_attribute_with_no_args_when_creating_then_it_must_have_default_formatting_rules()
    {
        var idGenerator = new CompactEzIdGenerator<DefaultFormattedCompactEzId>(1);
        var id = idGenerator.GetNextId();

        id.Value.Length.Should().Be(15);
        id.Value[3].Should().Be('-');
        id.Value[11].Should().Be('-');
    }

    [Fact]
    public void Given_id_with_attribute_with_separator_value_and_positions_when_creating_then_it_must_have_correct_formatting_rules()
    {
        var idGenerator = new CompactEzIdGenerator<CompactEzIdWithDash>(1);
        var id = idGenerator.GetNextId();

        id.Value.Length.Should().Be(15);
        id.Value[4].Should().Be('-');
        id.Value[9].Should().Be('-');
    }

    [Fact]
    public void Given_id_with_attribute_with_no_separators_when_creating_then_it_must_have_no_separators()
    {
        var idGenerator = new CompactEzIdGenerator<CompactEzIdWithNoSeparator>(1);
        var id = idGenerator.GetNextId();

        id.Value.Length.Should().Be(13);
    }

    [Fact]
    public void Given_id_with_attribute_with_under_score_separator_when_creating_then_it_must_have_under_score_separators()
    {
        var idGenerator = new CompactEzIdGenerator<CompactEzIdWithUnderscore>(1);
        var id = idGenerator.GetNextId();

        id.Value.Length.Should().Be(15);
        id.Value[3].Should().Be('_');
        id.Value[11].Should().Be('_');
    }
}
