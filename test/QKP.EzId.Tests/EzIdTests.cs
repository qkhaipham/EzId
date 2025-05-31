using FluentAssertions;

namespace QKP.EzId.Tests
{
    public class EzIdTests
    {
        [Theory]
        [InlineData(123456789, "2Q6-NP1R000-000")]
        [InlineData(987654321, "P5M-DWEG000-000")]
        [InlineData(long.MaxValue, "ZZZ-ZZZZZZZ-ZQY")]
        [InlineData(long.MinValue, "000-0000000-080")]
        [InlineData(0, "000-0000000-000")]
        public void Given_long_value_when_creating_ezid_it_must_encode_correctly(long value, string expected)
        {
            var ezId = new EzId(value);

            ezId.ToString().Should().Be(expected);
            ezId.Value.Should().Be(expected);
        }

        [Fact]
        public void Given_ezid_when_converting_to_string_and_parsing_back_it_must_return_equivalent_ezid()
        {
            var originalEzId = new EzId(124567890);
            string strVal = originalEzId.ToString();

            var parsedEzId = EzId.Parse(strVal);

            parsedEzId.Should().Be(originalEzId);
            parsedEzId.Value.Should().Be(originalEzId.Value);
        }

        [Theory]
        [InlineData("2Q6-NP1R000-000", 123456789)]
        [InlineData("P5M-DWEG000-000", 987654321)]
        [InlineData("ZZZ-ZZZZZZZ-ZQY", long.MaxValue)]
        [InlineData("000-0000000-080", long.MinValue)]
        [InlineData("000-0000000-000", 0)]
        public void Given_valid_string_when_parsing_it_must_return_expected_value(string input, long expected)
        {
            var ezId = EzId.Parse(input);
            var expectedEzId = new EzId(expected);

            ezId.Should().Be(expectedEzId);
            ezId.Value.Should().Be(input);
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid")]
        [InlineData("000-000000-000")] // Too short
        [InlineData("0000-0000000-000")] // Wrong format
        [InlineData("000-0000000-0000")] // Too long
        [InlineData("UUU-UUUUUUU-UUU")] // Invalid characters (U is not in Crockford base32)
        public void Given_invalid_string_when_parsing_it_must_throw_exception(string invalidInput)
        {
            Action act = () => EzId.Parse(invalidInput);

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Given_valid_string_when_using_tryparse_it_must_return_true_and_correct_ezid()
        {
            var originalEzId = new EzId(124567890);
            string strVal = originalEzId.ToString();

            bool success = EzId.TryParse(strVal, null, out var parsedEzId);

            success.Should().BeTrue();
            parsedEzId.Should().Be(originalEzId);
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid")]
        [InlineData("000-000000-000")] // Too short
        [InlineData("0000-0000000-000")] // Wrong format
        [InlineData("000-0000000-0000")] // Too long
        [InlineData("UUU-UUUUUUU-UUU")] // Invalid characters
        [InlineData(null)] // Null string
        public void Given_invalid_string_when_using_tryparse_it_must_return_false_and_error_id(string? invalidInput)
        {
            bool success = EzId.TryParse(invalidInput, null, out var result);

            success.Should().BeFalse();
            result.Should().Be(EzId.ErrorId);
        }

        [Fact]
        public void Given_two_ezids_with_same_value_when_comparing_they_must_be_equal()
        {
            var ezId1 = new EzId(12345);
            var ezId2 = new EzId(12345);

            ezId1.Should().Be(ezId2);
            ezId1.Equals(ezId2).Should().BeTrue();
            (ezId1 == ezId2).Should().BeTrue();
            (ezId1 != ezId2).Should().BeFalse();
            ezId1.GetHashCode().Should().Be(ezId2.GetHashCode());
        }

        [Fact]
        public void Given_two_ezids_with_different_values_when_comparing_they_must_not_be_equal()
        {
            var ezId1 = new EzId(12345);
            var ezId2 = new EzId(54321);

            ezId1.Should().NotBe(ezId2);
            ezId1.Equals(ezId2).Should().BeFalse();
            (ezId1 == ezId2).Should().BeFalse();
            (ezId1 != ezId2).Should().BeTrue();
        }

        [Fact]
        public void Given_ezid_and_null_when_comparing_they_must_not_be_equal()
        {
            var ezId = new EzId(12345);

            ezId.Equals(null).Should().BeFalse();
            (ezId == null).Should().BeFalse();
            (null == ezId).Should().BeFalse();
            (ezId != null).Should().BeTrue();
            (null != ezId).Should().BeTrue();
        }

        [Fact]
        public void Given_null_ezids_when_comparing_they_must_be_equal()
        {
            EzId? ezId1 = null;
            EzId? ezId2 = null;

            (ezId1 == ezId2).Should().BeTrue();
            (ezId1 != ezId2).Should().BeFalse();
        }

        [Fact]
        public void Given_ezid_and_different_type_when_comparing_they_must_not_be_equal()
        {
            var ezId = new EzId(12345);
            var differentType = "12345";

            // ReSharper disable once SuspiciousTypeConversion.Global
            ezId.Equals(differentType).Should().BeFalse();
        }

        [Fact]
        public void Given_ezid_when_comparing_with_itself_it_must_be_equal()
        {
            var ezId = new EzId(12345);

            ezId.Equals(ezId).Should().BeTrue();
            // ReSharper disable once EqualExpressionComparison
            (ezId == ezId).Should().BeTrue();
            // ReSharper disable once EqualExpressionComparison
            (ezId != ezId).Should().BeFalse();
        }
    }
}
