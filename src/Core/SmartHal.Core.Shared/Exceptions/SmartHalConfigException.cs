namespace SmartHal.Core;

/// <summary>
/// Base exception for configuration-related errors.
/// </summary>
public class SmartHalConfigException : SmartHalException
{
    public SmartHalConfigException(string message) : base(message)
    {
    }

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

    public SmartHalConfigValidationException(string message, IReadOnlyList<string> validationErrors)
        : base(message)
    {
        ValidationErrors = validationErrors;
    }
}
