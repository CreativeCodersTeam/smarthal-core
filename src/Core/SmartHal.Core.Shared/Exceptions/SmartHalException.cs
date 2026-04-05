namespace SmartHal.Core;

/// <summary>
/// Base exception for all SmartHal-specific errors.
/// </summary>
public class SmartHalException : Exception
{
    public SmartHalException(string message) : base(message)
    {
    }

    public SmartHalException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
