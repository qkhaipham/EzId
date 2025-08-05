using System;
using System.Linq;

namespace QKP.EzId;

/// <summary>
/// Attribute used to mark a struct as an ID type. The source generator will generate the implementation.
/// </summary>
[AttributeUsage(AttributeTargets.Struct)]
public class EzIdTypeAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the bit size for the ID.
    /// </summary>
    public IdBitSize BitSize { get; set; } = IdBitSize.Bits96;

    /// <summary>
    /// Gets or sets the separator character used in the ID string representation.
    /// </summary>
    public SeparatorOptions Separator { get; set; } = SeparatorOptions.Dash;

    /// <summary>
    /// Gets or sets the separator positions.
    /// </summary>
    public int[] SeparatorPositions { get; set; } = { 5, 15 };

    /// <summary>
    /// Initializes a new instance of the <see cref="EzIdTypeAttribute"/> class.
    /// </summary>
    public EzIdTypeAttribute()
    {
    }

    /// <summary>
    /// Initializes an instance of the <see cref="EzIdTypeAttribute"/> class.
    /// </summary>
    /// <param name="bitSize">The bit size of the generated ID without separators.</param>
    /// <param name="separator">The separator.</param>
    /// <param name="separatorPositions">The separator positions</param>
    public EzIdTypeAttribute(IdBitSize bitSize, SeparatorOptions separator, int[] separatorPositions)
    {
        BitSize = bitSize;
        Separator = separator;
        SeparatorPositions = separatorPositions.Distinct().ToArray();
    }
}

/// <summary>
/// Enumeration representing the different types of separators that can be used in the ID string representation.
/// </summary>
public enum IdBitSize
{
    /// <summary>
    /// 96 bits for the ID without separators.
    /// </summary>
    Bits96 = 96,
    /// <summary>
    /// 64 bits for the ID without separators.
    /// </summary>
    Bits64 = 64
}

/// <summary>
/// Enumeration representing the different types of separators that can be used in the ID string representation.
/// </summary>
public enum SeparatorOptions
{
    /// <summary>
    /// No separator will be used in the ID string representation.
    /// </summary>
    None = 0,
    /// <summary>
    /// A dash (-) will be used as the separator in the ID string representation.
    /// </summary>
    Dash = 1,
    /// <summary>
    /// An underscore (_) will be used as the separator in the ID string representation.
    /// </summary>
    Underscore = 2
}
