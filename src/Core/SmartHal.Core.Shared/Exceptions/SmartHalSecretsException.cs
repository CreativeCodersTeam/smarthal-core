namespace SmartHal.Core;

/// <summary>
/// Base exception for secrets-related errors.
/// </summary>
public class SmartHalSecretsException : SmartHalException
{
    public SmartHalSecretsException(string message) : base(message)
    {
    }

    public SmartHalSecretsException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Thrown when a requested secret cannot be found.
/// </summary>
public class SmartHalSecretNotFoundException : SmartHalSecretsException
{
    /// <summary>
    /// Gets the key of the secret that was not found.
    /// </summary>
    public string SecretKey { get; }

    public SmartHalSecretNotFoundException(string key)
        : base($"Secret '{key}' nicht gefunden.")
    {
        SecretKey = key;
    }
}

/// <summary>
/// Thrown when a secrets provider encounters an error.
/// </summary>
public class SmartHalSecretsProviderException : SmartHalSecretsException
{
    /// <summary>
    /// Gets the name of the provider that caused the error.
    /// </summary>
    public string ProviderName { get; }

    public SmartHalSecretsProviderException(string message, string providerName)
        : base(message)
    {
        ProviderName = providerName;
    }

    public SmartHalSecretsProviderException(string message, string providerName, Exception innerException)
        : base(message, innerException)
    {
        ProviderName = providerName;
    }
}
