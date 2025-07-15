using System;

namespace QKP.EzId;

/// <summary>
/// An exception that is thrown when the clock has moved backwards when the day lights saving change.
/// </summary>
public class DayLightSavingChangedException : Exception
{
    /// <summary>
    /// An exception that is thrown when the clock has moved backwards when the day lights saving change.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public DayLightSavingChangedException(string message) : base(message)
    {
    }
}