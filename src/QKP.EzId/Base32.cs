using System;
using System.Collections.Generic;
using System.Text;

namespace QKP.EzId
{
    /// <summary>
    /// Encoder for base32.
    /// </summary>
    public class Base32
    {
        /// <summary>
        /// Encodes base32 with the crockford alphabet which excludes I, L, O and U.
        /// </summary>
        public static readonly Base32 Base32CrockFord = new Base32(new Alphabet("0123456789ABCDEFGHJKMNPQRSTVWXYZ"));

        private Base32(Alphabet alphabet)
        {
            Alphabet = alphabet;
        }

        /// <summary>
        /// Gets the alphabet used for the encoding.
        /// </summary>
        public Alphabet Alphabet { get; }

        /// <summary>
        /// Encodes a <see cref="ReadOnlySpan{T}"/> of <see cref="byte"/>.
        /// </summary>
        /// <param name="value">The input value.</param>
        /// <returns>A base 32 encoded representation of the input value of type <see cref="string"/>.</returns>
        public string Encode(ReadOnlySpan<byte> value)
        {
            var buffer = 0;
            var bufferLength = 0;
            const int byteLength = 8;
            const int base32Bits = 5;

            var sb = new StringBuilder();

            foreach (byte b in value)
            {
                buffer = (buffer << byteLength) | b;
                bufferLength += byteLength;

                while (bufferLength >= base32Bits)
                {
                    bufferLength -= base32Bits;
                    int index = (buffer >> bufferLength) & 0x1F;
                    sb.Append(Alphabet.Characters[index]);
                }
            }

            if (bufferLength > 0)
            {
                int index = (buffer << (base32Bits - bufferLength)) & 0x1F;
                sb.Append(Alphabet.Characters[index]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Encodes a <see cref="long"/>.
        /// </summary>
        /// <param name="value">The input value.</param>
        /// <returns>A base 32 encoded representation of the input value of type <see cref="string"/>.</returns>
        public string Encode(long value)
        {
            return Encode(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Decodes a <see cref="string"/> value to a <see cref="long"/> representation.
        /// </summary>
        /// <param name="value">The <see cref="string"/> value.</param>
        /// <returns>The decoded value represented as a <see cref="long"/>.</returns>
        public long DecodeLong(string value)
        {
            byte[] bytes = Decode(value);
            return BitConverter.ToInt64(bytes, 0);
        }

        /// <summary>
        /// Decodes a <see cref="string"/> value to a <see cref="byte"/> array representation.
        /// </summary>
        /// <param name="value">The <see cref="string"/> value.</param>
        /// <returns>The decoded value represented as a <see cref="byte"/> array.</returns>
        public byte[] Decode(string value)
        {
            List<byte> bytes = new List<byte>();
            int val = 0;
            int bits = 0;

            foreach (char character in value)
            {
                if (!Alphabet.ReverseTable.TryGetValue(character, out int bitValue))
                {
                    throw new FormatException($"Invalid character '{character}' is not part of the alphabet ['{Alphabet.Characters}'].");
                }

                val = (val << 5) | bitValue;
                bits += 5;

                if (bits >= 8)
                {
                    bits -= 8;
                    bytes.Add((byte)((val >> bits) & 0xFF));
                }
            }

            if (bits > 0)
            {
                bytes.Add((byte)((val << (8 - bits)) & 0xFF));
            }

            return bytes.ToArray();
        }
    }
}
