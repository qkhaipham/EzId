using FluentAssertions;
using QKP.EzId.CustomTypes;
using Xunit;

namespace QKP.EzId.Integration.Tests;

public class Bits64SourceGeneratedIdsTests
{
    [Fact]
    public void Given_id_attribute_with_bit_size_64_and_separator_value_and_positions_when_creating_then_it_must_have_correct_formatting_rules()
    {
        var idGenerator = new CompactEzIdGenerator<CompactEzIdWithDash>(1);
        var id = idGenerator.GetNextId();

        id.Value.Length.Should().Be(15);
        id.Value[4].Should().Be('-');
        id.Value[9].Should().Be('-');
    }

    [Fact]
    public void Given_id_with_attribute_with_bit_size_64_and_no_separators_when_creating_then_it_must_have_no_separators()
    {
        var idGenerator = new CompactEzIdGenerator<CompactEzIdWithNoSeparator>(1);
        var id = idGenerator.GetNextId();

        id.Value.Length.Should().Be(13);
    }

    [Fact]
    public void Given_id_with_attribute_with_bit_size_64_with_under_score_separator_when_creating_then_it_must_have_under_score_separators()
    {
        var idGenerator = new CompactEzIdGenerator<CompactEzIdWithUnderscore>(1);
        var id = idGenerator.GetNextId();

        id.Value.Length.Should().Be(15);
        id.Value[3].Should().Be('_');
        id.Value[11].Should().Be('_');
    }

    [Fact]
    public void Given_id_with_attribute_with_named_args_for_bit_size_64_with_under_score_separator_when_creating_then_it_must_have_under_score_separators()
    {
        var idGenerator = new CompactEzIdGenerator<CompactEzIdWithNamedArgs>(1);
        var id = idGenerator.GetNextId();

        id.Value.Length.Should().Be(15);
        id.Value[3].Should().Be('_');
        id.Value[11].Should().Be('_');
    }
}
