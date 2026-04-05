namespace SmartHal.Core.Secrets;

/// <summary>
/// Provides access to secrets for adapter configurations.
/// Credentials are never stored in YAML files — instead, adapter configs
/// reference secret keys that are resolved at runtime through a provider.
/// </summary>
public interface ISecretsProvider
{
    /// <summary>Gets the provider name (e.g. "env", "file", "windows", "macos", "linux").</summary>
    string ProviderName { get; }

    /// <summary>Retrieves a secret value by key.</summary>
    /// <exception cref="SmartHalSecretNotFoundException">Thrown when the key does not exist.</exception>
    Task<string> GetSecretAsync(string key, CancellationToken ct = default);

    /// <summary>Stores a secret value.</summary>
    Task SetSecretAsync(string key, string value, CancellationToken ct = default);

    /// <summary>Deletes a secret by key.</summary>
    Task DeleteSecretAsync(string key, CancellationToken ct = default);

    /// <summary>Lists all available secret keys.</summary>
    Task<IReadOnlyList<string>> ListKeysAsync(CancellationToken ct = default);

    /// <summary>Checks whether a secret with the given key exists.</summary>
    Task<bool> ExistsAsync(string key, CancellationToken ct = default);
}
