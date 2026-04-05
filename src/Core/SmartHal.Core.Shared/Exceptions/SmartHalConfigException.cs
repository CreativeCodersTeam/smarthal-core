namespace SmartHal.Core;

/// <summary>
/// Base exception for configuration-related errors.
/// </summary>
public class SmartHalConfigException : SmartHalException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SmartHalConfigException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public SmartHalConfigException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartHalConfigException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception that caused this error.</param>
    public SmartHalConfigException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Thrown when a configuration file cannot be read or parsed.
/// </summary>
public class SmartHalConfigFileException : SmartHalConfigException
{
    /// <summary>
    /// Gets the path of the configuration file that caused the error.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Gets the line number where the error occurred, if available.
    /// </summary>
    public int? LineNumber { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartHalConfigFileException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="filePath">The path of the configuration file that caused the error.</param>
    /// <param name="lineNumber">The line number where the error occurred, if available.</param>
    public SmartHalConfigFileException(string message, string filePath, int? lineNumber = null)
        : base(message)
    {
        FilePath = filePath;
        LineNumber = lineNumber;
    }
}

/// <summary>
/// Thrown when configuration validation fails.
/// </summary>
public class SmartHalConfigValidationException : SmartHalConfigException
{
    /// <summary>
    /// Gets the list of validation errors that were found.
    /// </summary>
    public IReadOnlyList<string> ValidationErrors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartHalConfigValidationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="validationErrors">The list of validation errors that were found.</param>
    public SmartHalConfigValidationException(string message, IReadOnlyList<string> validationErrors)
        : base(message)
    {
        ValidationErrors = validationErrors;
    }
}
