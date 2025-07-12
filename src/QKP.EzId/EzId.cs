using System;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using QKP.EzId.Json;

namespace QKP.EzId
{
    /// <summary>
    /// An identifier which encodes a 96-bit value with base32 crockford encoding
    /// to produce human friendly readable identifiers.
    /// 4 bytes ( 32 bits ) = timestamp in seconds since UNIX epoch
    /// 5 bytes = ( 40 bits ) = generatorId, a random value generated once per process
    /// 3 bytes = ( 24 bits ) = sequence, starts at a random value and increments for each ID generated
    /// <example>
    /// 070AB-47XF6Q8NH0-YPA40
    /// </example>
    /// </summary>
    [JsonConverter(typeof(EzIdJsonConverter))]
    public readonly struct EzId :
#if NET7_0_OR_GREATER
        ISpanParsable<EzId>,
#endif
        IEquatable<EzId>,
        IComparable<EzId>,
        IConvertible
    {
        private readonly int _start;
        private readonly int _mid;
        private readonly int _end;

        private static int s_sequence = new Random().Next();
        private static readonly long s_generatorId = GenerateRandomGeneratorId();

        private const char Separator = '-';
        private static readonly int[] s_separatorPositions = new[] { 5, 15 };
        private static readonly int s_length = 20 + s_separatorPositions.Length;

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

        /// <summary>
        /// Gets a default empty ID value.
        /// </summary>
        public static readonly EzId Empty = default;

        /// <summary>
        /// Gets the string value of <see cref="EzId"/>.
        /// </summary>
        public string Value { get; }

        private static long GenerateRandomGeneratorId()
        {
            var random = new Random();
            int high = random.Next();
            int low = random.Next();
            long combined = (long)((ulong)(uint)high << 32 | (uint)low);
            return combined & 0xffffffffff; // get lowest 5 bytes
        }

        private EzId(int start, int mid, int end)
        {
            _start = start;
            _mid = mid;
            _end = end;

            byte[] bytes = new byte[12];
            Buffer.BlockCopy(BitConverter.GetBytes(start), 0, bytes, 0, 4); // bytes 0-3
            Buffer.BlockCopy(BitConverter.GetBytes(mid), 0, bytes, 4, 4); // bytes 4-7
            Buffer.BlockCopy(BitConverter.GetBytes(end), 0, bytes, 8, 4); // bytes 8-11
            Value = Format(Base32.Base32CrockFord.Encode(bytes));
        }

        /// <summary>
        /// Creates an instance of <see cref="EzId"/>.
        /// </summary>
        /// <returns>An instance of <see cref="EzId"/>.</returns>
        public static EzId GetNextId()
        {
            int start = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            int mid = (int)s_generatorId >> 8; // get 32 highest bits
            int sequence =
                Interlocked.Increment(ref s_sequence) & 0xFFFFFF; // mask 24 bits of 64 to get 40 bits for sequence
            int end = (int)s_generatorId << 24 | sequence; // get highest 8 bits of generatorId and 24 bits of sequence

            return new EzId(start, mid, end);
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
            if (s.Length != s_length)
            {
                throw new ArgumentOutOfRangeException(nameof(s), $"Value must have a length equal to {s_length}.");
            }

            string encodedValue = s.Replace(Separator.ToString(), string.Empty);

            foreach (char c in encodedValue)
            {
                if (!Base32.Base32CrockFord.Alphabet.Characters.Contains(c))
                    throw new ArgumentOutOfRangeException(nameof(s), $"Value contains illegal character '{c}'.");
            }

            byte[] bytes = Base32.Base32CrockFord.Decode(encodedValue);
            if (bytes.Length != 12)
            {
                throw new ArgumentOutOfRangeException(nameof(s), "Decoded byte array must be 12 bytes.");
            }

            int start = BitConverter.ToInt32(bytes, 0);
            int mid = BitConverter.ToInt32(bytes, 4);
            int end = BitConverter.ToInt32(bytes, 8);
            return new EzId(start, mid, end);
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
                result = Empty;
                return false;
            }

            return true;
        }

#if NET7_0_OR_GREATER
        /// <summary>
        /// Parses a <see cref="ReadOnlySpan{T}"/> value to an instance of <see cref="EzId"/>.
        /// </summary>
        /// <param name="s">The <see cref="ReadOnlySpan{T}"/> value to parse.</param>
        /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
        /// <returns>The parsed <see cref="EzId"/> value.</returns>
        public static EzId Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => Parse(s.ToString(), provider);

        /// <summary>
        /// Parses a <see cref="ReadOnlySpan{T}"/> value where T is <see cref="char"/>to an instance of <see cref="EzId"/>.
        /// </summary>
        /// <param name="s">The <see cref="ReadOnlySpan{T}"/> where T is <see cref="char"/> value to parse.</param>
        /// <param name="provider">An object that provides culture-specific formatting information. This parameter is ignored.</param>
        /// <param name="result">When this method returns, contains the parsed value if successful, otherwise a default value.</param>
        /// <returns>true if parsing succeeded; otherwise, false.</returns>
        public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out EzId result) =>
            TryParse(s.ToString(), provider, out result);
#endif

        /// <summary>
        /// Returns the string representation of this <see cref="EzId"/>.
        /// </summary>
        public override string ToString() => Value;

        /// <summary>
        /// Compares this instance to another <see cref="EzId"/>.
        /// </summary>
        /// <param name="other">The other <see cref="EzId"/> to compare to.</param>
        /// <returns>An integer that indicates the relative order of the objects being compared.</returns>
        public int CompareTo(EzId other) => string.Compare(Value, other.Value, StringComparison.Ordinal);

        /// <summary>
        /// Determines whether this instance and a specified object, which must also be an <see cref="EzId"/>, have the same value.
        /// </summary>
        /// <param name="obj">The object to compare to this instance.</param>
        /// <returns>true if obj is an <see cref="EzId"/> and its value is the same as this instance; otherwise, false.</returns>
        public override bool Equals(object? obj) => obj is EzId other && Equals(other);

        /// <summary>
        /// Determines whether this instance and another specified <see cref="EzId"/> object have the same value.
        /// </summary>
        /// <param name="other">The <see cref="EzId"/> to compare to this instance.</param>
        /// <returns>true if the value of the other parameter is the same as this instance; otherwise, false.</returns>
        public bool Equals(EzId other) => Value == other.Value;

        /// <summary>
        /// Returns the hash code for this <see cref="EzId"/>.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode() => Value.GetHashCode();

        /// <summary>
        /// Determines whether two specified <see cref="EzId"/> objects have the same value.
        /// </summary>
        public static bool operator ==(EzId left, EzId right) => left.Equals(right);

        /// <summary>
        /// Determines whether two specified <see cref="EzId"/> objects have different values.
        /// </summary>
        public static bool operator !=(EzId left, EzId right) => !left.Equals(right);

        /// <summary>
        /// Determines whether one <see cref="EzId"/> is greater than another.
        /// </summary>
        public static bool operator >(EzId left, EzId right) => left.CompareTo(right) > 0;

        /// <summary>
        /// Determines whether one <see cref="EzId"/> is less than another.
        /// </summary>
        public static bool operator <(EzId left, EzId right) => left.CompareTo(right) < 0;

        /// <summary>
        /// Determines whether one <see cref="EzId"/> is greater than or equal to another.
        /// </summary>
        public static bool operator >=(EzId left, EzId right) => left.CompareTo(right) >= 0;

        /// <summary>
        /// Determines whether one <see cref="EzId"/> is less than or equal to another.
        /// </summary>
        public static bool operator <=(EzId left, EzId right) => left.CompareTo(right) <= 0;

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
                    if (conversionType == typeof(object) || conversionType == typeof(EzId))
                        return this;
                    break;
                case TypeCode.String:
                    return ToString(provider);
            }

            throw new InvalidCastException();
        }

        /// <inheritdoc />
        public ushort ToUInt16(IFormatProvider? provider) => throw new InvalidCastException();

        /// <inheritdoc />
        public uint ToUInt32(IFormatProvider? provider) => throw new InvalidCastException();

        /// <inheritdoc />
        public ulong ToUInt64(IFormatProvider? provider) => throw new InvalidCastException();
    }
}
