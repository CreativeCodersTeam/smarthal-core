namespace SmartHal.Core;

/// <summary>
/// Base exception for secrets-related errors.
/// </summary>
public class SmartHalSecretsException : SmartHalException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SmartHalSecretsException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public SmartHalSecretsException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartHalSecretsException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception that caused this error.</param>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartHalSecretNotFoundException"/> class.
    /// </summary>
    /// <param name="key">The key of the secret that was not found.</param>
    public SmartHalSecretNotFoundException(string key)
        : base($"Secret '{key}' was not found.")
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

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartHalSecretsProviderException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="providerName">The name of the provider that caused the error.</param>
    public SmartHalSecretsProviderException(string message, string providerName)
        : base(message)
    {
        ProviderName = providerName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartHalSecretsProviderException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="providerName">The name of the provider that caused the error.</param>
    /// <param name="innerException">The inner exception that caused this error.</param>
    public SmartHalSecretsProviderException(string message, string providerName, Exception innerException)
        : base(message, innerException)
    {
        ProviderName = providerName;
    }
}
