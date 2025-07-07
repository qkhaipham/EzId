using System;
using System.Linq;

namespace QKP.EzId
{
    /// <summary>
    /// Attribute used to mark a struct as an ID type. The source generator will generate the implementation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public class EzIdTypeAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the separator character used in the ID string representation.
        /// </summary>
        public SeparatorOptions Separator { get; set; } = SeparatorOptions.Dash;

        /// <summary>
        /// Gets or sets the separator positions.
        /// </summary>
        public int[] SeparatorPositions { get; set; } = { 3, 10};

        /// <summary>
        /// Initializes a new instance of the <see cref="EzIdTypeAttribute"/> class.
        /// </summary>
        public EzIdTypeAttribute()
        {
        }

        /// <summary>
        /// Initializes an instance of the <see cref="EzIdTypeAttribute"/> class.
        /// </summary>
        /// <param name="separator">The separator.</param>
        /// <param name="separatorPositions">The separator positions</param>
        public EzIdTypeAttribute(SeparatorOptions separator, int[] separatorPositions)
        {
            if (separator == SeparatorOptions.None && separatorPositions.Any())
            {
                throw new ArgumentException($"Invalid separator positions for separator {separator}", nameof(separatorPositions));
            }

            if (separator != SeparatorOptions.None)
            {
                if (!separatorPositions.All(x => x >= 0 && x <= 12))
                {
                    throw new ArgumentException($"Invalid separator positions for separator {separator}, separator positions must be a number between 0 and 12", nameof(separatorPositions));
                }
            }

            Separator = separator;
            SeparatorPositions = separatorPositions.Distinct().ToArray();
        }
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
}
