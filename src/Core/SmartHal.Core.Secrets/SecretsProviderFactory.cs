namespace SmartHal.Core.Secrets;

/// <summary>
/// Creates <see cref="ISecretsProvider"/> instances based on the provider name from configuration.
/// Supports automatic platform detection via the "auto" provider name.
/// </summary>
public class SecretsProviderFactory
{
    private readonly Func<Task<string>>? _passwordCallback;

    /// <summary>
    /// Creates a new factory instance.
    /// </summary>
    /// <param name="passwordCallback">
    /// Callback for the master password, required by the <see cref="EncryptedFileSecretsProvider"/>.
    /// May be <c>null</c> if only non-file providers are used.
    /// </param>
    public SecretsProviderFactory(Func<Task<string>>? passwordCallback = null)
    {
        _passwordCallback = passwordCallback;
    }

    /// <summary>
    /// Creates a secrets provider based on the given name.
    /// </summary>
    /// <param name="providerName">The provider name (e.g. "env", "file", "windows", "macos", "linux", "auto").</param>
    /// <param name="configPath">Optional config path for the file provider.</param>
    /// <returns>A configured <see cref="ISecretsProvider"/> instance.</returns>
    /// <exception cref="SmartHalSecretsProviderException">Thrown for unknown provider names.</exception>
    public ISecretsProvider Create(string providerName, string? configPath = null) => providerName switch
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

    private ISecretsProvider CreatePlatformProvider()
    {
        if (OperatingSystem.IsWindows())
        {
            return new WindowsCredentialManagerProvider();
        }

        if (OperatingSystem.IsMacOS())
        {
            return new MacOsKeychainProvider();
        }

        if (OperatingSystem.IsLinux())
        {
            return new LinuxSecretServiceProvider();
        }

        // Fallback to encrypted file provider
        return new EncryptedFileSecretsProvider(
            _passwordCallback ?? throw new SmartHalSecretsProviderException(
                "Password callback is required for the file provider fallback.", "auto"));
    }
}
