namespace SmartHal.Core;

/// <summary>
/// Defines exit codes for the SmartHal CLI application.
/// Each exception type maps to a specific exit code range.
/// </summary>
public static class ExitCodes
{
    public const int Success = 0;
    public const int GeneralError = 1;

    // Config errors (10-29)
    public const int ConfigError = 10;
    public const int ConfigFileError = 11;
    public const int ConfigValidationError = 12;

    // Adapter errors (30-59)
    public const int AdapterError = 30;
    public const int AdapterConnectionError = 31;
    public const int AdapterDeviceNotFound = 32;
    public const int AdapterOperationError = 33;

    // Secrets errors (60-89)
    public const int SecretsError = 60;
    public const int SecretNotFound = 61;
    public const int SecretsProviderError = 62;

    /// <summary>
    /// Determines the appropriate exit code for the given exception.
    /// More specific exceptions take precedence over their base types.
    /// </summary>
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
