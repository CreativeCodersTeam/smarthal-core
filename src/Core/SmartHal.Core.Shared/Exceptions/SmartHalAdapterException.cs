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

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartHalAdapterException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="adapterId">The identifier of the adapter that caused the error.</param>
    public SmartHalAdapterException(string message, string adapterId) : base(message)
    {
        AdapterId = adapterId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartHalAdapterException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="adapterId">The identifier of the adapter that caused the error.</param>
    /// <param name="innerException">The inner exception that caused this error.</param>
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
    /// <summary>
    /// Initializes a new instance of the <see cref="SmartHalAdapterConnectionException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="adapterId">The identifier of the adapter that caused the error.</param>
    public SmartHalAdapterConnectionException(string message, string adapterId)
        : base(message, adapterId)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartHalAdapterConnectionException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="adapterId">The identifier of the adapter that caused the error.</param>
    /// <param name="innerException">The inner exception that caused this error.</param>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartHalAdapterDeviceNotFoundException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="adapterId">The identifier of the adapter that caused the error.</param>
    /// <param name="nativeId">The native device identifier that was not found.</param>
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
    /// <summary>
    /// Initializes a new instance of the <see cref="SmartHalAdapterOperationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="adapterId">The identifier of the adapter that caused the error.</param>
    public SmartHalAdapterOperationException(string message, string adapterId)
        : base(message, adapterId)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartHalAdapterOperationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="adapterId">The identifier of the adapter that caused the error.</param>
    /// <param name="innerException">The inner exception that caused this error.</param>
    public SmartHalAdapterOperationException(string message, string adapterId, Exception innerException)
        : base(message, adapterId, innerException)
    {
    }
}
