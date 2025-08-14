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

        string? raw = reader.GetString();

        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new JsonException("CompactEzId cannot be empty.");
        }

        if (!CompactEzId.TryParse(raw, out var id))
        {
            throw new JsonException($"Invalid CompactEzId value: '{raw}'.");
        }

        return id;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, CompactEzId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
