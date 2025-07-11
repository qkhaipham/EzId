using System.Globalization;
using FluentAssertions;
using QKP.EzId.CustomTypes;
using Xunit;

namespace QKP.EzId.Integration.Tests;

public class Bits96SourceGeneratedIdsTests
{
    [Fact]
    public void Given_id_with_attribute_with_no_args_when_generating_then_it_must_have_default_formatting_rules()
    {
        var id = DefaultFormattedCompactEzId.Generate();

        id.ToString(CultureInfo.InvariantCulture).Length.Should().Be(22);
        id.Value[5].Should().Be('-');
        id.Value[16].Should().Be('-');
    }

    [Fact]
    public void Given_id_attribute_with_bit_size_96_and_separator_value_and_positions_when_generating_then_it_must_have_correct_formatting_rules()
    {
        var id = EzIdWithDash.Generate();

        id.Value.Length.Should().Be(22);
        id.Value[5].Should().Be('-');
        id.Value[16].Should().Be('-');
    }

    [Fact]
    public void Given_id_with_attribute_with_bit_size_96_and_no_separators_when_generating_then_it_must_have_no_separators()
    {
        var id = EzIdWithNoSeparator.Generate();

        id.Value.Length.Should().Be(20);
    }

    [Fact]
    public void Given_id_with_attribute_with_bit_size_96_and_under_score_separator_when_generating_then_it_must_have_under_score_separators()
    {
        var id = EzIdWithUnderscore.Generate();

        id.Value.Length.Should().Be(22);
        id.Value[3].Should().Be('_');
        id.Value[20].Should().Be('_');
    }

    [Fact]
    public void Given_id_with_attribute_with_named_args_for_bit_size_96_and_under_score_separator_when_generating_then_it_must_have_under_score_separators()
    {
        var id = EzIdWithNamedArgs.Generate();

        id.Value.Length.Should().Be(22);
        id.Value[3].Should().Be('_');
        id.Value[20].Should().Be('_');
    }
}
