namespace SmartHal.Core;

/// <summary>
/// Base exception for all SmartHal-specific errors.
/// </summary>
public class SmartHalException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SmartHalException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public SmartHalException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartHalException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception that caused this error.</param>
    public SmartHalException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
