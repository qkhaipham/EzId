namespace QKP.EzId;

/// <summary>
/// Marker interface for compact EzId types.
/// </summary>
/// <typeparam name="TSelf">The implementing type itself.</typeparam>
public interface ICompactEzId<TSelf> where TSelf : ICompactEzId<TSelf>
{
}
