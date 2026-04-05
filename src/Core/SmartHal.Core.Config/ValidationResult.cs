namespace SmartHal.Core.Config;

/// <summary>
/// Result of a configuration validation.
/// </summary>
public class ValidationResult
{
    /// <summary>Gets a value that indicates whether the validation passed without errors.</summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>Gets or sets the list of validation errors.</summary>
    public List<ValidationError> Errors { get; set; } = [];

    /// <summary>Gets or sets the list of validation warnings.</summary>
    public List<ValidationWarning> Warnings { get; set; } = [];
}

/// <summary>
/// Represents a validation error.
/// </summary>
public class ValidationError
{
    /// <summary>Gets or sets the error code.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Gets or sets the error message.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Gets or sets the file path where the error occurred.</summary>
    public string? FilePath { get; set; }

    /// <summary>Gets or sets the line number where the error occurred.</summary>
    public int? LineNumber { get; set; }
}

/// <summary>
/// Represents a validation warning.
/// </summary>
public class ValidationWarning
{
    /// <summary>Gets or sets the warning code.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Gets or sets the warning message.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Gets or sets the file path where the warning originated.</summary>
    public string? FilePath { get; set; }
}
