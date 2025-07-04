namespace QKP.EzId.SourceGenerator;

internal static partial class Templates
{
    public static string NewtonsoftJsonConverterImplementationTemplate =>
        """
        using System;
        using Newtonsoft.Json;

        #nullable enable

        namespace {Namespace}.Json
        {
            /// <summary>
            /// Newtonsoft Json converter for <see cref="{Namespace}.{TypeName}"/> to read and write into a primitive <see cref="string"/>.
            /// </summary>
            public class {TypeName}NewtonsoftJsonConverter : JsonConverter<{Namespace}.{TypeName}>
            {
                /// <inheritdoc />
                public override {Namespace}.{TypeName} ReadJson(JsonReader reader, Type objectType, {Namespace}.{TypeName} existingValue, bool hasExistingValue, JsonSerializer serializer)
                {
                    if (reader.TokenType != JsonToken.String)
                    {
                        throw new JsonSerializationException("Expected a string.");
                    }

                    return {Namespace}.{TypeName}.Parse((string)reader.Value!);
                }

                /// <inheritdoc />
                public override void WriteJson(JsonWriter writer, {Namespace}.{TypeName} value, JsonSerializer serializer)
                {
                    writer.WriteValue(value.Value);
                }
            }
        }
        """;
}
