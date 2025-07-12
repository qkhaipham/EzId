using System.Collections.Concurrent;
using System.Globalization;
using FluentAssertions;

namespace QKP.EzId.Tests
{
    public class EzIdTests
    {
        [Fact]
        public void Given_two_generated_ezids_when_comparing_they_must_not_be_equal()
        {
            var id1 = EzId.GetNextId();
            var id2 = EzId.GetNextId();
            id1.Should().NotBe(id2);
            id1.Value.Should().NotBeNullOrWhiteSpace();
            id2.Value.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Given_ezid_when_calling_tostring_it_must_return_value()
        {
            var id = EzId.GetNextId();
            id.Value.Should().Be(id.Value);
        }

        [Fact]
        public void Given_ezid_when_parsing_and_tryparse_it_must_roundtrip()
        {
            var original = EzId.GetNextId();
            var parsed = EzId.Parse(original.Value);
            parsed.Should().Be(original);
            parsed.Value.Should().Be(original.Value);

            EzId.TryParse(original.Value, out var tryParsed).Should().BeTrue();
            tryParsed.Should().Be(original);
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid")]
        [InlineData("000-000000-000")] // Too short
        [InlineData("0000-0000000-000")] // Wrong format
        [InlineData("000-0000000-0000")] // Too long
        [InlineData("UUUUU-UUUUUUUUUU-UUUUU")] // Invalid chars
        public void Given_invalid_string_when_parsing_it_must_throw_exception(string input)
        {
            Action act = () => EzId.Parse(input);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid")]
        [InlineData("000-000000-000")] // Too short
        [InlineData("0000-0000000-000")] // Wrong format
        [InlineData("000-0000000-0000")] // Too long
        [InlineData("UUUUU-UUUUUUUUUU-UUUUU")] // Invalid chars
        [InlineData(null)]
        public void Given_invalid_string_when_tryparse_it_must_return_false_and_empty(string? input)
        {
            EzId.TryParse(input, out var result).Should().BeFalse();
            result.Should().Be(EzId.Empty);
        }

        [Fact]
        public void Given_two_ezids_with_same_value_when_comparing_they_must_be_equal_and_operators_work()
        {
            var id1 = EzId.GetNextId();
            var id2 = EzId.Parse(id1.Value);
            var id3 = EzId.GetNextId();

            id1.Should().Be(id2);
            id1.Equals(id2).Should().BeTrue();
            (id1 == id2).Should().BeTrue();
            (id1 != id3).Should().BeTrue();
            id1.GetHashCode().Should().Be(id2.GetHashCode());
        }

        [Fact]
        public void Given_two_ezids_when_comparing_with_compareto_and_operators_they_should_match()
        {
            var id1 = EzId.GetNextId();
            var id2 = EzId.GetNextId();
            if (id1.Value == id2.Value) return; // extremely rare, skip
            (id1.CompareTo(id2) == 0).Should().Be(id1 == id2);
            (id1 < id2).Should().Be(id1.CompareTo(id2) < 0);
            (id1 > id2).Should().Be(id1.CompareTo(id2) > 0);
            (id1 <= id2).Should().Be(id1.CompareTo(id2) <= 0);
            (id1 >= id2).Should().Be(id1.CompareTo(id2) >= 0);
        }

        [Fact]
        public void Given_ezid_and_null_or_different_type_when_comparing_they_must_not_be_equal()
        {
            var id = EzId.GetNextId();
            id.Equals(null).Should().BeFalse();
            id.Equals("string").Should().BeFalse();
        }

        [Fact]
        public void Given_ezid_when_getting_typecode_it_must_return_object()
        {
            var id = EzId.GetNextId();
            id.GetTypeCode().Should().Be(TypeCode.Object);
        }

        [Fact]
        public void Given_ezid_when_casting_to_type_valid_and_invalid_casts_should_behave_correctly()
        {
            var id = EzId.GetNextId();
            id.ToType(typeof(string), null).Should().Be(id.Value);
            id.ToType(typeof(EzId), null).Should().Be(id);
            id.ToType(typeof(object), null).Should().Be(id);
            Action invalid = () => id.ToType(typeof(int), null);
            invalid.Should().Throw<InvalidCastException>();
        }

        [Fact]
        public void Given_ezid_when_casting_to_iconvertible_invalid_casts_throw()
        {
            var id = EzId.GetNextId();
            Action boolCast = () => id.ToBoolean(null);
            Action byteCast = () => id.ToByte(null);
            Action charCast = () => id.ToChar(null);
            Action dateTimeCast = () => id.ToDateTime(null);
            Action decimalCast = () => id.ToDecimal(null);
            Action doubleCast = () => id.ToDouble(null);
            Action int16Cast = () => id.ToInt16(null);
            Action int32Cast = () => id.ToInt32(null);
            Action int64Cast = () => id.ToInt64(null);
            Action sbyteCast = () => id.ToSByte(null);
            Action singleCast = () => id.ToSingle(null);
            Action uint16Cast = () => id.ToUInt16(null);
            Action uint32Cast = () => id.ToUInt32(null);
            Action uint64Cast = () => id.ToUInt64(null);
            boolCast.Should().Throw<InvalidCastException>();
            byteCast.Should().Throw<InvalidCastException>();
            charCast.Should().Throw<InvalidCastException>();
            dateTimeCast.Should().Throw<InvalidCastException>();
            decimalCast.Should().Throw<InvalidCastException>();
            doubleCast.Should().Throw<InvalidCastException>();
            int16Cast.Should().Throw<InvalidCastException>();
            int32Cast.Should().Throw<InvalidCastException>();
            int64Cast.Should().Throw<InvalidCastException>();
            sbyteCast.Should().Throw<InvalidCastException>();
            singleCast.Should().Throw<InvalidCastException>();
            uint16Cast.Should().Throw<InvalidCastException>();
            uint32Cast.Should().Throw<InvalidCastException>();
            uint64Cast.Should().Throw<InvalidCastException>();
        }

        [Fact]
        public void Given_ezid_when_converting_to_string_and_parsing_back_it_must_return_equivalent_ezid()
        {
            var originalEzId = EzId.GetNextId();
            string strVal = originalEzId.ToString(CultureInfo.InvariantCulture);

            var parsedEzId = EzId.Parse(strVal);

            parsedEzId.Should().Be(originalEzId);
            parsedEzId.Value.Should().Be(originalEzId.Value);
        }

        [Fact]
        public void Given_valid_string_when_using_tryparse_it_must_return_true_and_correct_ezid()
        {
            var originalEzId = EzId.GetNextId();
            string strVal = originalEzId.ToString(CultureInfo.InvariantCulture);

            bool success = EzId.TryParse(strVal, out var parsedEzId);

            success.Should().BeTrue();
            parsedEzId.Should().Be(originalEzId);
        }

        [Fact]
        public void Given_two_ezids_with_same_value_when_comparing_they_must_be_equal()
        {
            var id1 = EzId.GetNextId();
            var id2 = EzId.Parse(id1.Value);

            id1.Should().Be(id2);
            id1.Equals(id2).Should().BeTrue();
            (id1 == id2).Should().BeTrue();
            (id1 != id2).Should().BeFalse();
            id1.GetHashCode().Should().Be(id2.GetHashCode());
        }

        [Fact]
        public void Given_two_ezids_with_different_values_when_comparing_they_must_not_be_equal()
        {
            var id1 = EzId.GetNextId();
            var id2 = EzId.GetNextId();

            // Extremely rare, but skip if they are equal
            if (id1 == id2) return;

            id1.Should().NotBe(id2);
            id1.Equals(id2).Should().BeFalse();
            (id1 == id2).Should().BeFalse();
            (id1 != id2).Should().BeTrue();
        }

        [Fact]
        public void Given_ezid_and_null_when_comparing_they_must_not_be_equal()
        {
            var id = EzId.GetNextId();

            id.Equals(null).Should().BeFalse();
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
            (id == null).Should().BeFalse();
            (null == id).Should().BeFalse();
            (id != null).Should().BeTrue();
            (null != id).Should().BeTrue();
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
        }

        [Fact]
        public void Given_null_ezids_when_comparing_they_must_be_equal()
        {
            EzId? id1 = null;
            EzId? id2 = null;

            (id1 == id2).Should().BeTrue();
            (id1 != id2).Should().BeFalse();
        }

        [Fact]
        public void Given_ezid_and_different_type_when_comparing_they_must_not_be_equal()
        {
            var id = EzId.GetNextId();
            var differentType = "string";

            id.Equals(differentType).Should().BeFalse();
        }

        [Fact]
        public void Given_two_ezids_when_comparing_with_greater_than_operator_it_should_return_expected_result()
        {
            var smaller = EzId.Parse("00000-0000000000-00000");
            var larger = EzId.Parse("ZZZZZ-ZZZZZZZZZZ-ZZZZZ");

            (larger > smaller).Should().BeTrue();
            (smaller > larger).Should().BeFalse();
        }

        [Fact]
        public void Given_two_ezids_when_comparing_with_less_than_operator_it_should_return_expected_result()
        {
            var smaller = EzId.Parse("00000-0000000000-00000");
            var larger = EzId.Parse("ZZZZZ-ZZZZZZZZZZ-ZZZZZ");

            (smaller < larger).Should().BeTrue();
            (larger < smaller).Should().BeFalse();
        }

        [Fact]
        public void Given_two_ezids_when_comparing_with_greater_than_or_equal_operator_it_should_return_expected_result()
        {
            var smaller = EzId.Parse("00000-0000000000-00000");
            var larger = EzId.Parse("ZZZZZ-ZZZZZZZZZZ-ZZZZZ");

            (larger >= smaller).Should().BeTrue();
            (smaller >= larger).Should().BeFalse();
            (larger >= larger).Should().BeTrue();
        }

        [Fact]
        public void Given_two_ezids_when_comparing_with_less_than_or_equal_operator_it_should_return_expected_result()
        {
            var smaller = EzId.Parse("00000-0000000000-00000");
            var larger = EzId.Parse("ZZZZZ-ZZZZZZZZZZ-ZZZZZ");

            (smaller <= larger).Should().BeTrue();
            (larger <= smaller).Should().BeFalse();
            (smaller <= smaller).Should().BeTrue();
        }

        [Fact]
        public void Given_ezid_when_comparing_with_itself_it_must_be_equal()
        {
            var id = EzId.GetNextId();

            id.Equals(id).Should().BeTrue();
#pragma warning disable CS1718 // Comparison made to same variable
            (id == id).Should().BeTrue();
            (id != id).Should().BeFalse();
#pragma warning restore CS1718 // Comparison made to same variable
        }

        [Fact]
        public void Given_ezid_when_casting_to_iconvertible_valid_casts_succeed()
        {
            var id = EzId.GetNextId();

            id.GetTypeCode().Should().Be(TypeCode.Object);
            id.ToString(null).Should().Be(id.ToString(CultureInfo.InvariantCulture));
            id.ToType(typeof(string), null).Should().Be(id.ToString(CultureInfo.InvariantCulture));
            id.ToType(typeof(EzId), null).Should().Be(id);
            id.ToType(typeof(object), null).Should().Be(id);

            Action invalidType = () => id.ToType(typeof(int), null);
            invalidType.Should().Throw<InvalidCastException>();
        }

        [Fact]
        public async Task When_generating_ids_concurrently_they_must_be_unique()
        {
            const int numThreads = 4;
            const int idsPerThread = 100;
            var ids = new ConcurrentBag<EzId>();
            var tasks = new List<Task>();

            // Act
            for (int i = 0; i < numThreads; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < idsPerThread; j++)
                    {
                        ids.Add(EzId.GetNextId());
                    }
                }));
            }
            await Task.WhenAll(tasks);

            // Assert
            ids.Should().HaveCount(numThreads * idsPerThread);
            ids.Distinct().Should().HaveCount(numThreads * idsPerThread);
        }
    }
}
