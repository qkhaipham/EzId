using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QKP.EzId.Json;

/// <summary>
/// Json converter for <see cref="EzId"/> to read and write into a primitive <see cref="string"/>.
/// </summary>
public class EzIdJsonConverter : JsonConverter<EzId>
{
    /// <inheritdoc />
    public override EzId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Expected a string.");
        }

        string? raw = reader.GetString();

        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new JsonException("EzId cannot be empty.");
        }

        if (!EzId.TryParse(raw, out var id))
        {
            throw new JsonException($"Invalid EzId value: '{raw}'.");
        }

        return id;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, EzId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
