using CreativeCoders.Core;
using Microsoft.Extensions.Logging;

namespace SmartHal.Core.Secrets;

/// <summary>
/// Creates <see cref="ISecretsProvider"/> instances based on the provider name from configuration.
/// Supports automatic platform detection via the "auto" provider name.
/// </summary>
public class SecretsProviderFactory
{
    private readonly Func<Task<string>>? _passwordCallback;
    private readonly ILogger<SecretsProviderFactory> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecretsProviderFactory"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="passwordCallback">
    /// Callback for the master password, required by the <see cref="EncryptedFileSecretsProvider"/>.
    /// May be <see langword="null"/> if only non-file providers are used.
    /// </param>
    public SecretsProviderFactory(ILogger<SecretsProviderFactory> logger, Func<Task<string>>? passwordCallback = null)
    {
        _logger = Ensure.NotNull(logger);
        _passwordCallback = passwordCallback;
    }

    /// <summary>
    /// Creates a secrets provider based on the given name.
    /// </summary>
    /// <param name="providerName">The provider name (e.g. "env", "file", "windows", "macos", "linux", "auto").</param>
    /// <param name="configPath">The optional configuration path for the file provider.</param>
    /// <returns>A configured <see cref="ISecretsProvider"/> instance.</returns>
    /// <exception cref="SmartHalSecretsProviderException">The provider name is unknown.</exception>
    public ISecretsProvider Create(string providerName, string? configPath = null)
    {
        _logger.LogInformation("Creating secrets provider {ProviderName}", providerName);

        return providerName switch
        {
            "env" => new EnvironmentVariableSecretsProvider(),
            "file" => new EncryptedFileSecretsProvider(
                _passwordCallback ?? throw new SmartHalSecretsProviderException(
                    "Password callback is required for the file provider.", "file"),
                configPath),
            "windows" => new WindowsCredentialManagerProvider(),
            "macos" => new MacOsKeychainProvider(),
            "linux" => new LinuxSecretServiceProvider(),
            "auto" => CreatePlatformProvider(),
            _ => throw new SmartHalSecretsProviderException(
                $"Unknown secrets provider: '{providerName}'.", providerName)
        };
    }

    private ISecretsProvider CreatePlatformProvider()
    {
        _logger.LogDebug("Auto-detecting platform secrets provider");

        if (OperatingSystem.IsWindows())
        {
            _logger.LogDebug("Detected platform: {Platform}, using {ProviderName} provider", "Windows", "windows");
            return new WindowsCredentialManagerProvider();
        }

        if (OperatingSystem.IsMacOS())
        {
            _logger.LogDebug("Detected platform: {Platform}, using {ProviderName} provider", "macOS", "macos");
            return new MacOsKeychainProvider();
        }

        if (OperatingSystem.IsLinux())
        {
            _logger.LogDebug("Detected platform: {Platform}, using {ProviderName} provider", "Linux", "linux");
            return new LinuxSecretServiceProvider();
        }

        // Fallback to encrypted file provider
        _logger.LogDebug("Falling back to encrypted file provider");
        return new EncryptedFileSecretsProvider(
            _passwordCallback ?? throw new SmartHalSecretsProviderException(
                "Password callback is required for the file provider fallback.", "auto"));
    }
}
