namespace QKP.EzId.CustomTypes;

// Custom separator positions (XXXX-XXXX-XXXXXXX)
[EzIdType(IdBitSize.Bits64, SeparatorOptions.Dash, [4, 8])]
public partial struct CompactEzIdWithDash;

// No separators (XXXXXXXXXXXXXX)
[EzIdType(IdBitSize.Bits64, SeparatorOptions.None, [])]
public partial struct CompactEzIdWithNoSeparator;

// Underscore separators (XXX_XXXXXXX_XXX)
[EzIdType(IdBitSize.Bits64, SeparatorOptions.Underscore, [3, 10])]
public partial struct CompactEzIdWithUnderscore;

// Underscore separators (XXX_XXXXXXX_XXX)
[EzIdType(separatorPositions: [3, 10], separator: SeparatorOptions.Underscore, bitSize: IdBitSize.Bits64)]
public partial struct CompactEzIdWithNamedArgs;
