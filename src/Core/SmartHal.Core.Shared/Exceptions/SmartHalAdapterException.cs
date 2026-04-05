namespace SmartHal.Core;

/// <summary>
/// Base exception for adapter-related errors.
/// </summary>
public class SmartHalAdapterException : SmartHalException
{
    /// <summary>
    /// Gets the identifier of the adapter that caused the error.
    /// </summary>
    public string AdapterId { get; }

    public SmartHalAdapterException(string message, string adapterId) : base(message)
    {
        AdapterId = adapterId;
    }

    public SmartHalAdapterException(string message, string adapterId, Exception innerException)
        : base(message, innerException)
    {
        AdapterId = adapterId;
    }
}

/// <summary>
/// Thrown when an adapter cannot establish a connection.
/// </summary>
public class SmartHalAdapterConnectionException : SmartHalAdapterException
{
    public SmartHalAdapterConnectionException(string message, string adapterId)
        : base(message, adapterId)
    {
    }

    public SmartHalAdapterConnectionException(string message, string adapterId, Exception innerException)
        : base(message, adapterId, innerException)
    {
    }
}

/// <summary>
/// Thrown when a device cannot be found via the adapter.
/// </summary>
public class SmartHalAdapterDeviceNotFoundException : SmartHalAdapterException
{
    /// <summary>
    /// Gets the native device identifier that was not found.
    /// </summary>
    public string NativeId { get; }

    public SmartHalAdapterDeviceNotFoundException(string message, string adapterId, string nativeId)
        : base(message, adapterId)
    {
        NativeId = nativeId;
    }
}

/// <summary>
/// Thrown when an adapter operation fails.
/// </summary>
public class SmartHalAdapterOperationException : SmartHalAdapterException
{
    public SmartHalAdapterOperationException(string message, string adapterId)
        : base(message, adapterId)
    {
    }

    public SmartHalAdapterOperationException(string message, string adapterId, Exception innerException)
        : base(message, adapterId, innerException)
    {
    }
}
