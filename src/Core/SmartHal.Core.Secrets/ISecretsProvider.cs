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
    /// <param name="key">The secret key.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The secret value.</returns>
    /// <exception cref="SmartHalSecretNotFoundException">The key does not exist.</exception>
    Task<string> GetSecretAsync(string key, CancellationToken ct = default);

    /// <summary>Stores a secret value.</summary>
    /// <param name="key">The secret key.</param>
    /// <param name="value">The secret value to store.</param>
    /// <param name="ct">The cancellation token.</param>
    Task SetSecretAsync(string key, string value, CancellationToken ct = default);

    /// <summary>Deletes a secret by key.</summary>
    /// <param name="key">The secret key to delete.</param>
    /// <param name="ct">The cancellation token.</param>
    Task DeleteSecretAsync(string key, CancellationToken ct = default);

    /// <summary>Lists all available secret keys.</summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A read-only list of all secret keys.</returns>
    Task<IReadOnlyList<string>> ListKeysAsync(CancellationToken ct = default);

    /// <summary>Checks whether a secret with the given key exists.</summary>
    /// <param name="key">The secret key to check.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>
    /// <see langword="true"/> if the secret exists; otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> ExistsAsync(string key, CancellationToken ct = default);
}
