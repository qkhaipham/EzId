namespace QKP.EzId.SourceGenerator;


internal static partial class Templates
{
    /// <summary>
    /// Gets the CompactEzIdTypeImplementationTemplate ( 64 bits ).
    /// </summary>
    public static string CompactEzIdTypeImplementationTemplate =>
        """
        using System;
        using QKP.EzId;
        using System.Text;
        using System.Text.Json.Serialization;
        using {Namespace}.Json;

        #nullable enable

        namespace {Namespace}
        {
            [System.Diagnostics.DebuggerDisplay("{Value}")]
            [JsonConverter(typeof({TypeName}JsonConverter))]
            public readonly partial struct {TypeName} :
                #if NET7_0_OR_GREATER
                    ISpanParsable<{TypeName}>,
                #endif
                    IEquatable<{TypeName}>,
                    IComparable<{TypeName}>,
                    ICompactEzId<{TypeName}>,
                    IConvertible
            {
                /// <summary>
                /// Gets the base32 encoded string representation of the identifier.
                /// </summary>
                public string Value { get; }

                private const char Separator = '{Separator}';
                private static readonly int[] s_separatorPositions = {SeparatorPositions}
                private static readonly int s_length = {Length};

                /// <summary>
                /// Gets an empty ID value of <see cref="{TypeName}"/>.
                /// </summary>
                public static readonly {TypeName} Empty = new {TypeName}(0);

                /// <summary>
                /// Initializes a new instance of the <see cref="{TypeName}"/> struct.
                /// </summary>
                /// <param name="value">The 64-bit value.</param>
                public {TypeName}(long value)
                {
                    Value = Format(QKP.EzId.Base32.Base32CrockFord.Encode(value));
                }

                /// <summary>
                /// Parses a <see cref="string"/> value to an instance of <see cref="{TypeName}"/>.
                /// </summary>
                /// <param name="value">The <see cref="string"/> value to parse.</param>
                /// <returns>The parsed <see cref="{TypeName}"/> value.</returns>
                public static {TypeName} Parse(string value) => Parse(value, null);

                /// <summary>
                /// Parses a <see cref="string"/> value to an instance of <see cref="{TypeName}"/>.
                /// </summary>
                /// <param name="s">The <see cref="string"/> value to parse.</param>
                /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
                /// <returns>The parsed <see cref="{TypeName}"/> value.</returns>
                public static {TypeName} Parse(string s, IFormatProvider? provider)
                {
                    if (s.Length != s_length)
                    {
                        throw new ArgumentOutOfRangeException(nameof(s), $"Value must have a length equal to {s_length}.");
                    }

                    string encodedValue = s.Replace(Separator.ToString(), string.Empty);

                    foreach (char c in encodedValue)
                    {
                        if (!Base32.Base32CrockFord.Alphabet.Characters.Contains(c))
                        {
                            throw new ArgumentOutOfRangeException(nameof(s), $"Value contains illegal character '{c}'.");
                        }
                    }

                    return new {TypeName}(QKP.EzId.Base32.Base32CrockFord.DecodeLong(encodedValue));
                }

                /// <summary>
                /// Parses a <see cref="string"/> value to an instance of <see cref="{TypeName}"/>.
                /// </summary>
                /// <param name="value">The <see cref="string"/> value to parse.</param>
                /// <param name="result">When this method returns, contains the parsed value if successful, otherwise a default value.</param>
                /// <returns>true if parsing succeeded; otherwise, false.</returns>
                public static bool TryParse(string? value, out {TypeName} result)
                {
                    return TryParse(value, null, out result);
                }

                /// <summary>
                /// Parses a <see cref="string"/> value to an instance of <see cref="{TypeName}"/>.
                /// </summary>
                /// <param name="value">The <see cref="string"/> value to parse.</param>
                /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
                /// <param name="result">When this method returns, contains the parsed value if successful, otherwise a default value.</param>
                /// <returns>true if parsing succeeded; otherwise, false.</returns>
                public static bool TryParse(string? value, IFormatProvider? provider, out {TypeName} result)
                {
                    try
                    {
                        result = Parse(value ?? "");
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        result = Empty;
                        return false;
                    }

                    return true;
                }

                /// <inheritdoc />
                public override string ToString()
                {
                    return Value;
                }

                /// <inheritdoc />
                public int CompareTo({TypeName} other) => string.Compare(Value, other.Value, StringComparison.Ordinal);

                /// <inheritdoc />
                public override bool Equals(object? obj)
                {
                    return obj is {TypeName} other && Equals(other);
                }

                /// <inheritdoc />
                public bool Equals({TypeName} other)
                {
                    return Value == other.Value;
                }

                /// <inheritdoc />
                public override int GetHashCode()
                {
                    return Value.GetHashCode();
                }

                /// <summary>
                /// Determines whether two specified {TypeName} objects have the same value.
                /// </summary>
                /// <param name="left">The first {TypeName} to compare.</param>
                /// <param name="right">The second {TypeName} to compare.</param>
                /// <returns>true if the value of left is the same as the value of right; otherwise, false.</returns>
                public static bool operator ==({TypeName} left, {TypeName} right)
                {
                    return left.Equals(right);
                }

                /// <summary>
                /// Determines whether two specified {TypeName} objects have different values.
                /// </summary>
                /// <param name="left">The first {TypeName} to compare.</param>
                /// <param name="right">The second {TypeName} to compare.</param>
                /// <returns>true if the value of left is different from the value of right; otherwise, false.</returns>
                public static bool operator !=({TypeName} left, {TypeName} right)
                {
                    return !left.Equals(right);
                }

                /// <summary>
                /// Determines whether a {TypeName} and a null reference have the same value.
                /// </summary>
                /// <param name="left">The {TypeName} to compare.</param>
                /// <param name="right">The null reference to compare.</param>
                /// <returns>Always false since a {TypeName} value type can never equal null.</returns>
                public static bool operator ==({TypeName} left, {TypeName}? right)
                {
                    return right.HasValue && left.Equals(right.Value);
                }

                /// <summary>
                /// Determines whether a {TypeName} and a null reference have the same value.
                /// </summary>
                /// <param name="left">The null reference to compare.</param>
                /// <param name="right">The {TypeName} to compare.</param>
                /// <returns>Always false since a {TypeName} value type can never equal null.</returns>
                public static bool operator ==({TypeName}? left, {TypeName} right)
                {
                    return left.HasValue && left.Value.Equals(right);
                }

                /// <summary>
                /// Determines whether a {TypeName} and a null reference have different values.
                /// </summary>
                /// <param name="left">The {TypeName} to compare.</param>
                /// <param name="right">The null reference to compare.</param>
                /// <returns>Always true since a {TypeName} value type can never equal null.</returns>
                public static bool operator !=({TypeName} left, {TypeName}? right)
                {
                    return !(left == right);
                }

                /// <summary>
                /// Determines whether a {TypeName} and a null reference have different values.
                /// </summary>
                /// <param name="left">The null reference to compare.</param>
                /// <param name="right">The {TypeName} to compare.</param>
                /// <returns>Always true since a {TypeName} value type can never equal null.</returns>
                public static bool operator !=({TypeName}? left, {TypeName} right)
                {
                    return !(left == right);
                }

                /// <summary>
                /// Determines whether two nullable {TypeName} objects have the same value.
                /// </summary>
                /// <param name="left">The first nullable {TypeName} to compare.</param>
                /// <param name="right">The second nullable {TypeName} to compare.</param>
                /// <returns>true if both are null, or if both have values and those values are equal; otherwise, false.</returns>
                public static bool operator ==({TypeName}? left, {TypeName}? right) => Equals(left, right);

                /// <summary>
                /// Determines whether two nullable {TypeName} objects have different values.
                /// </summary>
                /// <param name="left">The first nullable {TypeName} to compare.</param>
                /// <param name="right">The second nullable {TypeName} to compare.</param>
                /// <returns>true if one is null and the other is not, or if both have values and those values are not equal; otherwise, false.</returns>
                public static bool operator !=({TypeName}? left, {TypeName}? right) => !Equals(left, right);

                /// <summary>
                /// Determines whether one {TypeName} is greater than another.
                /// </summary>
                public static bool operator >({TypeName} left, {TypeName} right)
                {
                    return left.CompareTo(right) > 0;
                }

                /// <summary>
                /// Determines whether one {TypeName} is less than another.
                /// </summary>
                public static bool operator <({TypeName} left, {TypeName} right)
                {
                    return left.CompareTo(right) < 0;
                }

                /// <summary>
                /// Determines whether one {TypeName} is greater than or equal to another.
                /// </summary>
                public static bool operator >=({TypeName} left, {TypeName} right)
                {
                    return left.CompareTo(right) >= 0;
                }

                /// <summary>
                /// Determines whether one {TypeName} is less than or equal to another.
                /// </summary>
                public static bool operator <=({TypeName} left, {TypeName} right)
                {
                    return left.CompareTo(right) <= 0;
                }

                private static string Format(string encodedValue)
                {
                    var sb = new StringBuilder();
                    int currentSeparatorIndex = 0;

                    for (int i = 0; i < encodedValue.Length; i++)
                    {
                        if (currentSeparatorIndex < s_separatorPositions.Length &&
                            i == s_separatorPositions[currentSeparatorIndex])
                        {
                            sb.Append(Separator);
                            currentSeparatorIndex++;
                        }
                        sb.Append(encodedValue[i]);
                    }
                    return sb.ToString();
                }

        #if NET7_0_OR_GREATER
                /// <inheritdoc />
                public static {TypeName} Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => Parse(s.ToString(), provider);

                /// <inheritdoc />
                public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out {TypeName} result) =>
                    TryParse(s.ToString(), out result);
        #endif

                /// <inheritdoc />
                public TypeCode GetTypeCode() => TypeCode.Object;

                /// <inheritdoc />
                public bool ToBoolean(IFormatProvider? provider) => throw new InvalidCastException();

                /// <inheritdoc />
                public byte ToByte(IFormatProvider? provider) => throw new InvalidCastException();

                /// <inheritdoc />
                public char ToChar(IFormatProvider? provider) => throw new InvalidCastException();

                /// <inheritdoc />
                public DateTime ToDateTime(IFormatProvider? provider) => throw new InvalidCastException();

                /// <inheritdoc />
                public decimal ToDecimal(IFormatProvider? provider) => throw new InvalidCastException();

                /// <inheritdoc />
                public double ToDouble(IFormatProvider? provider) => throw new InvalidCastException();

                /// <inheritdoc />
                public short ToInt16(IFormatProvider? provider) => throw new InvalidCastException();

                /// <inheritdoc />
                public int ToInt32(IFormatProvider? provider) => throw new InvalidCastException();

                /// <inheritdoc />
                public long ToInt64(IFormatProvider? provider) => throw new InvalidCastException();

                /// <inheritdoc />
                public sbyte ToSByte(IFormatProvider? provider) => throw new InvalidCastException();

                /// <inheritdoc />
                public float ToSingle(IFormatProvider? provider) => throw new InvalidCastException();

                /// <inheritdoc />
                public string ToString(IFormatProvider? provider) => ToString();

                /// <inheritdoc />
                public object ToType(Type conversionType, IFormatProvider? provider)
                {
                    var code = Type.GetTypeCode(conversionType);
                    switch (code)
                    {
                        case TypeCode.Object:
                            if (conversionType == typeof(object) || conversionType == typeof({TypeName}))
                            {
                                return this;
                            }
                            break;
                        case TypeCode.String:
                            return ToString(provider);
                    }

                    throw new InvalidCastException();
                }

                /// <inheritdoc />
                public ushort ToUInt16(IFormatProvider? provider) => throw new InvalidCastException();

                /// <inheritdoc />
                public uint ToUInt32(IFormatProvider? provider) => throw new NotImplementedException();

                /// <inheritdoc />
                public ulong ToUInt64(IFormatProvider? provider) => throw new NotImplementedException();
            }
        }
        """;
}
