using FluentAssertions;
using Xunit;

namespace QKP.EzId.SourceGenerator.Tests
{
    public class EzIdTypeAttributeTests
    {
        [Fact]
        public void Given_default_constructor_when_creating_attribute_then_should_have_default_values()
        {
            // Act
            var attribute = new EzIdTypeAttribute();

            // Assert
            attribute.BitSize.Should().Be(IdBitSize.Bits96);
            attribute.Separator.Should().Be(SeparatorOptions.Dash);
            attribute.SeparatorPositions.Should().BeEquivalentTo([5, 15]);
        }

        [Fact]
        public void Given_64_bits_size_and_separator_and_positions_when_creating_attribute_then_should_set_bit_size_and_separator_and_positions()
        {
            // Act
            var attribute = new EzIdTypeAttribute(IdBitSize.Bits64, SeparatorOptions.Dash, [3, 10]);

            // Assert
            attribute.BitSize.Should().Be(IdBitSize.Bits64);
            attribute.Separator.Should().Be(SeparatorOptions.Dash);
            attribute.SeparatorPositions.Should().BeEquivalentTo([3, 10]);
        }

        [Fact]
        public void Given_96_bits_size_and_separator_and_positions_when_creating_attribute_then_should_set_bit_size_and_separator_and_positions()
        {
            // Act
            var attribute = new EzIdTypeAttribute(IdBitSize.Bits96, SeparatorOptions.Dash, [5, 15]);

            // Assert
            attribute.BitSize.Should().Be(IdBitSize.Bits96);
            attribute.Separator.Should().Be(SeparatorOptions.Dash);
            attribute.SeparatorPositions.Should().BeEquivalentTo([5, 15]);
        }

        [Fact]
        public void Given_attribute_usage_when_checked_then_should_only_allow_on_structs()
        {
            // Act
            var attributeUsage = Attribute.GetCustomAttribute(
                typeof(EzIdTypeAttribute),
                typeof(AttributeUsageAttribute)) as AttributeUsageAttribute;
            // Assert
            attributeUsage.Should().NotBeNull();
            attributeUsage!.ValidOn.Should().Be(AttributeTargets.Struct);
            attributeUsage.AllowMultiple.Should().BeFalse();
        }
    }
}
