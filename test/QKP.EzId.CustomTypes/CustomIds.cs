
namespace QKP.EzId.CustomTypes;

// Default format (XXX-XXXXXXX-XXX)
[EzIdType]
public partial struct DefaultFormattedEzId { }

// Custom separator positions (XXXX-XXXX-XXXXXXX)
[EzIdType(SeparatorOptions.Dash, [4, 8])]
public partial struct EzIdWithDash { }

// No separators (XXXXXXXXXXXXXX)
[EzIdType(SeparatorOptions.None, [])]
public partial struct EzIdWithNoSeparator { }

// Underscore separators (XXX_XXXXXXX_XXX)
[EzIdType(SeparatorOptions.Underscore, [3, 10])]
public partial struct EzIdWithUnderscore { }
