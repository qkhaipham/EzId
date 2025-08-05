namespace QKP.EzId;

/// <summary>
/// Provides tick values for time-based operations.
/// </summary>
public interface ITickProvider
{
    /// <summary>
    /// Gets the current tick value.
    /// </summary>
    /// <returns>The current tick value in milliseconds.</returns>
    long GetTick();
}
