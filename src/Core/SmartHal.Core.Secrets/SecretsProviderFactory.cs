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
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecretsProviderFactory"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="loggerFactory">The logger factory for creating typed loggers for child providers.</param>
    /// <param name="passwordCallback">
    /// Callback for the master password, required by the <see cref="EncryptedFileSecretsProvider"/>.
    /// May be <see langword="null"/> if only non-file providers are used.
    /// </param>
    public SecretsProviderFactory(
        ILogger<SecretsProviderFactory> logger,
        ILoggerFactory loggerFactory,
        Func<Task<string>>? passwordCallback = null)
    {
        _logger = Ensure.NotNull(logger);
        _loggerFactory = Ensure.NotNull(loggerFactory);
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
            "env" => new EnvironmentVariableSecretsProvider(
                _loggerFactory.CreateLogger<EnvironmentVariableSecretsProvider>()),
            "file" => new EncryptedFileSecretsProvider(
                _passwordCallback ?? throw new SmartHalSecretsProviderException(
                    "Password callback is required for the file provider.", "file"),
                _loggerFactory.CreateLogger<EncryptedFileSecretsProvider>(),
                configPath),
            "windows" => new WindowsCredentialManagerProvider(
                _loggerFactory.CreateLogger<WindowsCredentialManagerProvider>()),
            "macos" => new MacOsKeychainProvider(
                _loggerFactory.CreateLogger<MacOsKeychainProvider>()),
            "linux" => new LinuxSecretServiceProvider(
                _loggerFactory.CreateLogger<LinuxSecretServiceProvider>()),
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
            return new WindowsCredentialManagerProvider(
                _loggerFactory.CreateLogger<WindowsCredentialManagerProvider>());
        }

        if (OperatingSystem.IsMacOS())
        {
            _logger.LogDebug("Detected platform: {Platform}, using {ProviderName} provider", "macOS", "macos");
            return new MacOsKeychainProvider(
                _loggerFactory.CreateLogger<MacOsKeychainProvider>());
        }

        if (OperatingSystem.IsLinux())
        {
            _logger.LogDebug("Detected platform: {Platform}, using {ProviderName} provider", "Linux", "linux");
            return new LinuxSecretServiceProvider(
                _loggerFactory.CreateLogger<LinuxSecretServiceProvider>());
        }

        // Fallback to encrypted file provider
        _logger.LogDebug("Falling back to encrypted file provider");
        return new EncryptedFileSecretsProvider(
            _passwordCallback ?? throw new SmartHalSecretsProviderException(
                "Password callback is required for the file provider fallback.", "auto"),
            _loggerFactory.CreateLogger<EncryptedFileSecretsProvider>());
    }
}
