using System.Globalization;
using FluentAssertions;
using Xunit;


namespace QKP.EzId.SourceGenerator.Integration.Tests;

// Default format (XXXXX-XXXXXXXXXX-XXXXX)
[EzIdType(IdBitSize.Bits96, SeparatorOptions.Dash, [5, 15])]
public partial struct DefaultFormattedCompactEzId;

// Custom separator positions (XXXX-XXXX-XXXXXXX)
[EzIdType(IdBitSize.Bits96, SeparatorOptions.Dash, [5, 15])]
public partial struct EzIdWithDash;

// No separators (XXXXXXXXXXXXXXXXXXXX)
[EzIdType(IdBitSize.Bits96, SeparatorOptions.None, [])]
public partial struct EzIdWithNoSeparator;

// Underscore separators (XXX_XXXXXXXXXXXXXXXX_X)
[EzIdType(IdBitSize.Bits96, SeparatorOptions.Underscore, [3, 19])]
public partial struct EzIdWithUnderscore;

// Underscore separators (XXX_XXXXXXXXXXXXXXXX_X)
[EzIdType(separatorPositions: [3, 19], separator: SeparatorOptions.Underscore, bitSize: IdBitSize.Bits96)]
public partial struct EzIdWithNamedArgs;

public class Bits96SourceGeneratedIdsTests
{
    [Fact]
    public void Given_id_with_attribute_with_no_args_when_generating_then_it_must_have_default_formatting_rules()
    {
        var id = DefaultFormattedCompactEzId.GetNextId();

        id.ToString(CultureInfo.InvariantCulture).Length.Should().Be(22);
        id.Value[5].Should().Be('-');
        id.Value[16].Should().Be('-');
    }

    [Fact]
    public void Given_id_attribute_with_bit_size_96_and_separator_value_and_positions_when_generating_then_it_must_have_correct_formatting_rules()
    {
        var id = EzIdWithDash.GetNextId();

        id.Value.Length.Should().Be(22);
        id.Value[5].Should().Be('-');
        id.Value[16].Should().Be('-');
    }

    [Fact]
    public void Given_id_with_attribute_with_bit_size_96_and_no_separators_when_generating_then_it_must_have_no_separators()
    {
        var id = EzIdWithNoSeparator.GetNextId();

        id.Value.Length.Should().Be(20);
    }

    [Fact]
    public void Given_id_with_attribute_with_bit_size_96_and_under_score_separator_when_generating_then_it_must_have_under_score_separators()
    {
        var id = EzIdWithUnderscore.GetNextId();

        id.Value.Length.Should().Be(22);
        id.Value[3].Should().Be('_');
        id.Value[20].Should().Be('_');
    }

    [Fact]
    public void Given_id_with_attribute_with_named_args_for_bit_size_96_and_under_score_separator_when_generating_then_it_must_have_under_score_separators()
    {
        var id = EzIdWithNamedArgs.GetNextId();

        id.Value.Length.Should().Be(22);
        id.Value[3].Should().Be('_');
        id.Value[20].Should().Be('_');
    }
}
