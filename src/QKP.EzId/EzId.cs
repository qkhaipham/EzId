﻿using System;
using System.Globalization;
using System.Linq;
using System.Text;
#if NET7_0_OR_GREATER
using System.Numerics;
#endif

namespace QKP.EzId
{
    /// <summary>
    /// An identifier which encodes a 64-bit value with base32 crockford encoding
    /// to produce human friendly readable identifiers.
    ///
    /// <example>
    /// 070-47XF6Q8-YPA
    /// </example>
    /// </summary>
#if NET7_0_OR_GREATER
    public readonly struct EzId : IEquatable<EzId>, IParsable<EzId>
#else
    public readonly struct EzId : IEquatable<EzId>
#endif
    {
        /// <summary>
        /// Gets the base32 encoded string representation of the identifier.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets a default error ID value.
        /// </summary>
        public static readonly EzId ErrorId = new EzId(0);

        private const char Separator = '-';
        private const int Length = 15;

        /// <summary>
        /// Initializes a new instance of the <see cref="EzId"/> struct.
        /// </summary>
        /// <param name="value">The 64-bit value.</param>
        public EzId(long value)
        {
            Value = Format(Base32.Base32CrockFord.Encode(value));
        }

        /// <summary>
        /// Parses a <see cref="string"/> value to an instance of <see cref="EzId"/>.
        /// </summary>
        /// <param name="value">The <see cref="string"/> value to parse.</param>
        /// <returns>The parsed <see cref="EzId"/> value.</returns>
        public static EzId Parse(string value) => Parse(value, null);

        /// <summary>
        /// Parses a <see cref="string"/> value to an instance of <see cref="EzId"/>.
        /// </summary>
        /// <param name="s">The <see cref="string"/> value to parse.</param>
        /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
        /// <returns>The parsed <see cref="EzId"/> value.</returns>
        public static EzId Parse(string s, IFormatProvider? provider)
        {
            if (s.Length != Length)
            {
                throw new ArgumentOutOfRangeException(nameof(s), $"Value must have a length equal to {Length}.");
            }

            string encodedValue = s.Replace(Separator.ToString(), string.Empty);

            foreach (char c in encodedValue)
            {
                if (!Base32.Base32CrockFord.Alphabet.Characters.Contains(c))
                {
                    throw new ArgumentOutOfRangeException(nameof(s), $"Value contains illegal character '{c}'.");
                }
            }

            return new EzId(Base32.Base32CrockFord.DecodeLong(encodedValue));
        }

        /// <summary>
        /// Parses a <see cref="string"/> value to an instance of <see cref="EzId"/>.
        /// </summary>
        /// <param name="value">The <see cref="string"/> value to parse.</param>
        /// <param name="result">When this method returns, contains the parsed value if successful, otherwise a default value.</param>
        /// <returns>true if parsing succeeded; otherwise, false.</returns>
        public static bool TryParse(string? value, out EzId result)
        {
            return TryParse(value, null, out result);
        }

        /// <summary>
        /// Parses a <see cref="string"/> value to an instance of <see cref="EzId"/>.
        /// </summary>
        /// <param name="value">The <see cref="string"/> value to parse.</param>
        /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
        /// <param name="result">When this method returns, contains the parsed value if successful, otherwise a default value.</param>
        /// <returns>true if parsing succeeded; otherwise, false.</returns>
        public static bool TryParse(string? value, IFormatProvider? provider, out EzId result)
        {
            try
            {
                result = Parse(value ?? "");
            }
            catch (ArgumentOutOfRangeException)
            {
                result = ErrorId;
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
        public override bool Equals(object? obj)
        {
            return obj is EzId other && Equals(other);
        }

        /// <inheritdoc />
        public bool Equals(EzId other)
        {
            return Value == other.Value;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <summary>
        /// Determines whether two specified EzId objects have the same value.
        /// </summary>
        /// <param name="left">The first EzId to compare.</param>
        /// <param name="right">The second EzId to compare.</param>
        /// <returns>true if the value of left is the same as the value of right; otherwise, false.</returns>
        public static bool operator ==(EzId left, EzId right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two specified EzId objects have different values.
        /// </summary>
        /// <param name="left">The first EzId to compare.</param>
        /// <param name="right">The second EzId to compare.</param>
        /// <returns>true if the value of left is different from the value of right; otherwise, false.</returns>
        public static bool operator !=(EzId left, EzId right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Determines whether an EzId and a null reference have the same value.
        /// </summary>
        /// <param name="left">The EzId to compare.</param>
        /// <param name="right">The null reference to compare.</param>
        /// <returns>Always false since an EzId value type can never equal null.</returns>
        public static bool operator ==(EzId left, EzId? right)
        {
            return right.HasValue && left.Equals(right.Value);
        }

        /// <summary>
        /// Determines whether an EzId and a null reference have different values.
        /// </summary>
        /// <param name="left">The null reference to compare.</param>
        /// <param name="right">The EzId to compare.</param>
        /// <returns>Always true since an EzId value type can never equal null.</returns>
        public static bool operator ==(EzId? left, EzId right)
        {
            return left.HasValue && left.Value.Equals(right);
        }

        /// <summary>
        /// Determines whether an EzId and a null reference have different values.
        /// </summary>
        /// <param name="left">The EzId to compare.</param>
        /// <param name="right">The null reference to compare.</param>
        /// <returns>Always true since an EzId value type can never equal null.</returns>
        public static bool operator !=(EzId left, EzId? right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Determines whether an EzId and a null reference have different values.
        /// </summary>
        /// <param name="left">The null reference to compare.</param>
        /// <param name="right">The EzId to compare.</param>
        /// <returns>Always true since an EzId value type can never equal null.</returns>
        public static bool operator !=(EzId? left, EzId right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Determines whether two nullable EzId objects have the same value.
        /// </summary>
        /// <param name="left">The first nullable EzId to compare.</param>
        /// <param name="right">The second nullable EzId to compare.</param>
        /// <returns>true if both are null, or if both have values and those values are equal; otherwise, false.</returns>
        public static bool operator ==(EzId? left, EzId? right) => Equals(left, right);

        /// <summary>
        /// Determines whether two nullable EzId objects have different values.
        /// </summary>
        /// <param name="left">The first nullable EzId to compare.</param>
        /// <param name="right">The second nullable EzId to compare.</param>
        /// <returns>true if one is null and the other is not, or if both have values and those values are not equal; otherwise, false.</returns>
        public static bool operator !=(EzId? left, EzId? right) => !Equals(left, right);

        private static string Format(string encodedValue)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < encodedValue.Length; i++)
            {
                if (i is 3 || i is 10)
                {
                    sb.Append(Separator);
                }
                sb.Append(encodedValue[i]);
            }
            return sb.ToString();
        }
    }
}
