using CreativeCoders.Core;
using GitCredentialManager;
using Microsoft.Extensions.Logging;

namespace SmartHal.Core.Secrets;

/// <summary>
/// Stores secrets in the macOS Keychain via <c>Devlooped.CredentialManager</c>.
/// Throws <see cref="PlatformNotSupportedException"/> on non-macOS platforms.
/// </summary>
public class MacOsKeychainProvider : ISecretsProvider
{
    private readonly ICredentialStore _store;
    private readonly ILogger<MacOsKeychainProvider> _logger;

    /// <inheritdoc />
    public string ProviderName => "macos";

    /// <summary>
    /// Initializes a new instance of the <see cref="MacOsKeychainProvider"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="credentialNamespace">The credential namespace to use. Defaults to <c>SmartHal</c>.</param>
    /// <exception cref="PlatformNotSupportedException">The current platform is not macOS.</exception>
    public MacOsKeychainProvider(
        ILogger<MacOsKeychainProvider> logger,
        string credentialNamespace = "SmartHal")
    {
        _logger = Ensure.NotNull(logger);

        if (!OperatingSystem.IsMacOS())
        {
            throw new PlatformNotSupportedException("MacOsKeychainProvider is only supported on macOS.");
        }

        _store = CredentialManager.Create(credentialNamespace);
    }

    /// <inheritdoc />
    public Task<string> GetSecretAsync(string key, CancellationToken ct = default)
    {
        _logger.LogDebug("Getting secret {Key} from macOS Keychain", key);

        var credential = _store.Get(key, account: null);

        return credential is not null
            ? Task.FromResult(credential.Password)
            : throw new SmartHalSecretNotFoundException(key);
    }

    /// <inheritdoc />
    public Task SetSecretAsync(string key, string value, CancellationToken ct = default)
    {
        _logger.LogDebug("Setting secret {Key} in macOS Keychain", key);

        _store.AddOrUpdate(key, account: key, value);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteSecretAsync(string key, CancellationToken ct = default)
    {
        _logger.LogDebug("Deleting secret {Key} from macOS Keychain", key);

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
