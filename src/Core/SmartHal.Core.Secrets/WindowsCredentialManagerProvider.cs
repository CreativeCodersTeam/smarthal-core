using GitCredentialManager;

namespace SmartHal.Core.Secrets;

/// <summary>
/// Stores secrets in the Windows Credential Manager via <c>Devlooped.CredentialManager</c>.
/// Throws <see cref="PlatformNotSupportedException"/> on non-Windows platforms.
/// </summary>
public class WindowsCredentialManagerProvider : ISecretsProvider
{
    private readonly ICredentialStore _store;

    /// <inheritdoc />
    public string ProviderName => "windows";

    /// <summary>
    /// Creates a new Windows Credential Manager provider.
    /// </summary>
    /// <param name="credentialNamespace">The credential namespace to use. Defaults to <c>SmartHal</c>.</param>
    public WindowsCredentialManagerProvider(string credentialNamespace = "SmartHal")
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("WindowsCredentialManagerProvider is only supported on Windows.");
        }

        _store = CredentialManager.Create(credentialNamespace);
    }

    /// <inheritdoc />
    public Task<string> GetSecretAsync(string key, CancellationToken ct = default)
    {
        var credential = _store.Get(key, account: null);

        return credential is not null
            ? Task.FromResult(credential.Password)
            : throw new SmartHalSecretNotFoundException(key);
    }

    /// <inheritdoc />
    public Task SetSecretAsync(string key, string value, CancellationToken ct = default)
    {
        _store.AddOrUpdate(key, account: key, value);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteSecretAsync(string key, CancellationToken ct = default)
    {
        _store.Remove(key, account: key);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<string>> ListKeysAsync(CancellationToken ct = default)
    {
        // ICredentialStore does not support listing — return empty list
        return Task.FromResult<IReadOnlyList<string>>([]);
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        var credential = _store.Get(key, account: null);
        return Task.FromResult(credential is not null);
    }
}
