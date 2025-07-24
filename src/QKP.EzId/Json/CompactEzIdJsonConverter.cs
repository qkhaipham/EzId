using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QKP.EzId.Json;

/// <summary>
/// Json converter for <see cref="CompactEzId"/> to read and write into a primitive <see cref="string"/>.
/// </summary>
public class CompactEzIdJsonConverter : JsonConverter<CompactEzId>
{
    /// <inheritdoc />
    public override CompactEzId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Expected a string.");
        }

        return CompactEzId.Parse(reader.GetString()!);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, CompactEzId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
