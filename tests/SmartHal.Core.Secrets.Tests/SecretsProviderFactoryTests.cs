using AwesomeAssertions;

namespace SmartHal.Core.Secrets;

public class SecretsProviderFactoryTests
{
    [Fact]
    public void Create_Env_ReturnsEnvironmentVariableProvider()
    {
        var factory = new SecretsProviderFactory();

        var provider = factory.Create("env");

        provider.Should().BeOfType<EnvironmentVariableSecretsProvider>();
    }

    [Fact]
    public void Create_File_ReturnsEncryptedFileProvider()
    {
        var factory = new SecretsProviderFactory(() => Task.FromResult("password"));

        var provider = factory.Create("file");

        provider.Should().BeOfType<EncryptedFileSecretsProvider>();
    }

    [Fact]
    public void Create_File_WithoutPasswordCallback_ThrowsSecretsProviderException()
    {
        var factory = new SecretsProviderFactory();

        var act = () => factory.Create("file");

        act.Should().Throw<SmartHalSecretsProviderException>();
    }

    [Fact]
    public void Create_UnknownProvider_ThrowsSecretsProviderException()
    {
        var factory = new SecretsProviderFactory();

        var act = () => factory.Create("unknown");

        act.Should().Throw<SmartHalSecretsProviderException>();
    }

    [Fact]
    public void Create_Auto_OnMacOs_ReturnsMacOsProvider()
    {
        if (!OperatingSystem.IsMacOS())
        {
            return; // Skip on non-macOS
        }

        var factory = new SecretsProviderFactory();

        var provider = factory.Create("auto");

        provider.Should().BeOfType<MacOsKeychainProvider>();
    }

    [Fact]
    public void Create_Windows_OnNonWindows_ThrowsPlatformNotSupportedException()
    {
        if (OperatingSystem.IsWindows())
        {
            return; // Skip on Windows
        }

        var factory = new SecretsProviderFactory();

        var act = () => factory.Create("windows");

        act.Should().Throw<PlatformNotSupportedException>();
    }

    [Fact]
    public void Create_Linux_OnNonLinux_ThrowsPlatformNotSupportedException()
    {
        if (OperatingSystem.IsLinux())
        {
            return; // Skip on Linux
        }

        var factory = new SecretsProviderFactory();

        var act = () => factory.Create("linux");

        act.Should().Throw<PlatformNotSupportedException>();
    }
}
