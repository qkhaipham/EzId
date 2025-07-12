using System.Globalization;
using System.Text.Json;
using FluentAssertions;
using QKP.EzId.Json;

namespace QKP.EzId.Tests.Json;

public class CompactEzIdJsonConverterTests
{
    private record Person(CompactEzId Id, string Name);

    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void Given_json_with_id_as_string_when_deserializing_then_it_must_deserialize_as_expected()
    {
        var json = @"{""id"":""070-47XF6Q8-YPA"",""name"":""John Doe""}";

        // Act
        var result = JsonSerializer.Deserialize<Person>(json, _options);

        // Assert
        result.Should().NotBeNull();
        result!.Id.ToString(CultureInfo.InvariantCulture).Should().Be("070-47XF6Q8-YPA");
    }

    [Fact]
    public void Given_compact_ez_id_when_serializing_then_it_must_serialize_as_expected()
    {
        CompactEzId id = CompactEzId.Parse("070-47XF6Q8-YPA");
        var person = new Person(id, "John Doe");
        string expectedJson = @"{""id"":""070-47XF6Q8-YPA"",""name"":""John Doe""}";

        // Act
        string result = JsonSerializer.Serialize(person, _options);

        // Assert
        result.Should().Be(expectedJson);
    }
}


