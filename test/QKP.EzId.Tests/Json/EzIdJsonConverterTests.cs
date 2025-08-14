using System.Globalization;
using System.Text.Json;
using FluentAssertions;

namespace QKP.EzId.Tests.Json;

public class EzIdJsonConverterTests
{
    private record Person(EzId Id, string Name);

    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    [Fact]
    public void Given_json_with_id_as_string_when_deserializing_then_it_must_deserialize_as_expected()
    {
        var json = @"{""id"":""070XB-47XF6Q8FXG-69YP0"",""name"":""John Doe""}";

        // Act
        var result = JsonSerializer.Deserialize<Person>(json, _options);

        // Assert
        result.Should().NotBeNull();
        result!.Id.ToString(CultureInfo.InvariantCulture).Should().Be("070XB-47XF6Q8FXG-69YP0");
    }

    [Fact]
    public void Given_json_with_id_as_invalid_string_when_deserializing_then_it_must_throw_json_exception()
    {
        var json = @"{""id"":""invalid-id"",""name"":""John Doe""}";

        // Act
        Action act = () => JsonSerializer.Deserialize<Person>(json, _options);

        // Assert
        act.Should().Throw<JsonException>()
            .WithMessage("Invalid EzId value: 'invalid-id'.");
    }

    [Fact]
    public void Given_ez_id_when_serializing_then_it_must_serialize_as_expected()
    {
        EzId id = EzId.Parse("070XB-47XF6Q8FXG-69YP0");
        var person = new Person(id, "John Doe");
        string expectedJson = @"{""id"":""070XB-47XF6Q8FXG-69YP0"",""name"":""John Doe""}";

        // Act
        string result = JsonSerializer.Serialize(person, _options);

        // Assert
        result.Should().Be(expectedJson);
    }
}


