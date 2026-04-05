namespace SmartHal.Core;

/// <summary>
/// Defines exit codes for the SmartHal CLI application.
/// Each exception type maps to a specific exit code range.
/// </summary>
public static class ExitCodes
{
    /// <summary>The operation completed successfully.</summary>
    public const int Success = 0;

    /// <summary>A general error occurred.</summary>
    public const int GeneralError = 1;

    /// <summary>A configuration error occurred.</summary>
    public const int ConfigError = 10;

    /// <summary>A configuration file could not be read or parsed.</summary>
    public const int ConfigFileError = 11;

    /// <summary>Configuration validation failed.</summary>
    public const int ConfigValidationError = 12;

    /// <summary>An adapter error occurred.</summary>
    public const int AdapterError = 30;

    /// <summary>An adapter could not establish a connection.</summary>
    public const int AdapterConnectionError = 31;

    /// <summary>A device could not be found via the adapter.</summary>
    public const int AdapterDeviceNotFound = 32;

    /// <summary>An adapter operation failed.</summary>
    public const int AdapterOperationError = 33;

    /// <summary>A secrets-related error occurred.</summary>
    public const int SecretsError = 60;

    /// <summary>A requested secret was not found.</summary>
    public const int SecretNotFound = 61;

    /// <summary>A secrets provider encountered an error.</summary>
    public const int SecretsProviderError = 62;

    /// <summary>
    /// Determines the appropriate exit code for the given exception.
    /// More specific exceptions take precedence over their base types.
    /// </summary>
    /// <param name="ex">The exception to map to an exit code.</param>
    /// <returns>The exit code corresponding to the exception type.</returns>
    public static int FromException(Exception ex) => ex switch
    {
        SmartHalConfigFileException => ConfigFileError,
        SmartHalConfigValidationException => ConfigValidationError,
        SmartHalConfigException => ConfigError,
        SmartHalAdapterConnectionException => AdapterConnectionError,
        SmartHalAdapterDeviceNotFoundException => AdapterDeviceNotFound,
        SmartHalAdapterOperationException => AdapterOperationError,
        SmartHalAdapterException => AdapterError,
        SmartHalSecretNotFoundException => SecretNotFound,
        SmartHalSecretsProviderException => SecretsProviderError,
        SmartHalSecretsException => SecretsError,
        SmartHalException => GeneralError,
        _ => GeneralError
    };
}
