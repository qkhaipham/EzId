namespace QKP.EzId.SourceGenerator;

internal static partial class Templates
{
    public static string JsonConverterImplementationTemplate =>
        """
        #if NET5_0_OR_GREATER
        using System;
        using System.Text.Json;
        using System.Text.Json.Serialization;

        #nullable enable

        namespace {Namespace}.Json
        {
            /// <summary>
            /// Json converter for <see cref="{Namespace}.{TypeName}"/> to read and write into a primitive <see cref="string"/>.
            /// </summary>
            [JsonConverter(typeof({TypeName}JsonConverter))]
            public class {TypeName}JsonConverter : JsonConverter<{Namespace}.{TypeName}>
            {
                /// <inheritdoc />
                public override {Namespace}.{TypeName} Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                {
                    if (reader.TokenType != JsonTokenType.String)
                    {
                        throw new JsonException("Expected a string.");
                    }

                    return {Namespace}.{TypeName}.Parse(reader.GetString()!);
                }

                /// <inheritdoc />
                public override void Write(Utf8JsonWriter writer, {Namespace}.{TypeName} value, JsonSerializerOptions options)
                {
                    writer.WriteStringValue(value.Value);
                }
            }
        }
        #endif

        """;
}
