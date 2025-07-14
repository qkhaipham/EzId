namespace QKP.EzId.SourceGenerator;

internal static class Constants
{
    public const int Base32BitsPerChar = 5;
    public static class BitSize
    {
        public const int Bits96 = 96;
        public const int Bits64 = 64;
    }

    public static class SeparatorEnumValues
    {
        public const int None = 0;
        public const int Dash = 1;
        public const int Underscore = 2;
    }
}
