namespace SmartHal.Core.Secrets;

/// <summary>
/// Reads secrets from environment variables. Variables are mapped by convention:
/// secret key <c>homematic-eg.api_key</c> maps to <c>SMARTHAL_HOMEMATIC_EG_API_KEY</c>.
/// This provider is read-only — <see cref="SetSecretAsync"/> and <see cref="DeleteSecretAsync"/> throw.
/// </summary>
public class EnvironmentVariableSecretsProvider : ISecretsProvider
{
    private const string Prefix = "SMARTHAL_";

    /// <inheritdoc />
    public string ProviderName => "env";

    /// <inheritdoc />
    public Task<string> GetSecretAsync(string key, CancellationToken ct = default)
    {
        var envVar = SecretKeyValidator.ToEnvironmentVariable(key);
        var value = Environment.GetEnvironmentVariable(envVar);

        return value is not null
            ? Task.FromResult(value)
            : throw new SmartHalSecretNotFoundException(key);
    }

    /// <inheritdoc />
    public Task SetSecretAsync(string key, string value, CancellationToken ct = default) =>
        throw new SmartHalSecretsProviderException(
            "Environment variable secrets provider is read-only.", ProviderName);

    /// <inheritdoc />
    public Task DeleteSecretAsync(string key, CancellationToken ct = default) =>
        throw new SmartHalSecretsProviderException(
            "Environment variable secrets provider is read-only.", ProviderName);

    /// <inheritdoc />
    public Task<IReadOnlyList<string>> ListKeysAsync(CancellationToken ct = default)
    {
        var keys = Environment.GetEnvironmentVariables()
            .Keys
            .Cast<string>()
            .Where(k => k.StartsWith(Prefix, StringComparison.Ordinal))
            .Select(k => SecretKeyValidator.FromEnvironmentVariable(k))
            .Where(k => k is not null)
            .Cast<string>()
            .ToList();

        return Task.FromResult<IReadOnlyList<string>>(keys);
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        var envVar = SecretKeyValidator.ToEnvironmentVariable(key);
        return Task.FromResult(Environment.GetEnvironmentVariable(envVar) is not null);
    }
}
