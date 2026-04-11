using GitCredentialManager;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SmartHal.Core.Secrets;

/// <summary>
/// Stores secrets in the Windows Credential Manager via <c>Devlooped.CredentialManager</c>.
/// Throws <see cref="PlatformNotSupportedException"/> on non-Windows platforms.
/// </summary>
public class WindowsCredentialManagerProvider : ISecretsProvider
{
    private readonly ICredentialStore _store;
    private readonly ILogger<WindowsCredentialManagerProvider> _logger;

    /// <inheritdoc />
    public string ProviderName => "windows";

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsCredentialManagerProvider"/> class.
    /// </summary>
    /// <param name="logger">Optional logger instance.</param>
    /// <param name="credentialNamespace">The credential namespace to use. Defaults to <c>SmartHal</c>.</param>
    /// <exception cref="PlatformNotSupportedException">The current platform is not Windows.</exception>
    public WindowsCredentialManagerProvider(
        ILogger<WindowsCredentialManagerProvider>? logger = null,
        string credentialNamespace = "SmartHal")
    {
        _logger = logger ?? NullLogger<WindowsCredentialManagerProvider>.Instance;

        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("WindowsCredentialManagerProvider is only supported on Windows.");
        }

        _store = CredentialManager.Create(credentialNamespace);
    }

    /// <inheritdoc />
    public Task<string> GetSecretAsync(string key, CancellationToken ct = default)
    {
        _logger.LogDebug("Getting secret {Key} from Windows Credential Manager", key);

        var credential = _store.Get(key, account: null);

        return credential is not null
            ? Task.FromResult(credential.Password)
            : throw new SmartHalSecretNotFoundException(key);
    }

    /// <inheritdoc />
    public Task SetSecretAsync(string key, string value, CancellationToken ct = default)
    {
        _logger.LogDebug("Setting secret {Key} in Windows Credential Manager", key);

        _store.AddOrUpdate(key, account: key, value);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteSecretAsync(string key, CancellationToken ct = default)
    {
        _logger.LogDebug("Deleting secret {Key} from Windows Credential Manager", key);

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
