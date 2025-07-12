namespace QKP.EzId.CustomTypes;

// Default format (XXXXX-XXXXXXXXXX-XXXXX)
[EzIdType(IdBitSize.Bits96, SeparatorOptions.Dash, [5, 15])]
public partial struct DefaultFormattedCompactEzId;

// Custom separator positions (XXXX-XXXX-XXXXXXX)
[EzIdType(IdBitSize.Bits96, SeparatorOptions.Dash, [5, 15])]
public partial struct EzIdWithDash;

// No separators (XXXXXXXXXXXXXXXXXXXX)
[EzIdType(IdBitSize.Bits96, SeparatorOptions.None, [])]
public partial struct EzIdWithNoSeparator;

// Underscore separators (XXX_XXXXXXXXXXXXXXXX_X)
[EzIdType(IdBitSize.Bits96, SeparatorOptions.Underscore, [3, 19])]
public partial struct EzIdWithUnderscore;

// Underscore separators (XXX_XXXXXXXXXXXXXXXX_X)
[EzIdType(separatorPositions: [3, 19], separator: SeparatorOptions.Underscore, bitSize: IdBitSize.Bits96)]
public partial struct EzIdWithNamedArgs;
