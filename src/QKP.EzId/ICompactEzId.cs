namespace QKP.EzId
{
    /// <summary>
    /// Interface for ID types.
    /// </summary>
    /// <typeparam name="TSelf">The implementing type itself.</typeparam>
    public interface ICompactEzId<TSelf> where TSelf : ICompactEzId<TSelf>
    {
        /// <summary>
        /// Gets the string representation of the identifier.
        /// </summary>
        string Value { get; }
    }
}
