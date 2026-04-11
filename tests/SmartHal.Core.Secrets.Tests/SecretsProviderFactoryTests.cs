using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace SmartHal.Core.Secrets;

public class SecretsProviderFactoryTests
{
    [Fact]
    public void Create_Env_ReturnsEnvironmentVariableProvider()
    {
        // Arrange
        var factory = new SecretsProviderFactory(
            NullLogger<SecretsProviderFactory>.Instance, NullLoggerFactory.Instance);

        // Act
        var provider = factory.Create("env");

        // Assert
        provider.Should().BeOfType<EnvironmentVariableSecretsProvider>();
    }

    [Fact]
    public void Create_File_ReturnsEncryptedFileProvider()
    {
        // Arrange
        var factory = new SecretsProviderFactory(
            NullLogger<SecretsProviderFactory>.Instance,
            NullLoggerFactory.Instance,
            () => Task.FromResult("password"));

        // Act
        var provider = factory.Create("file");

        // Assert
        provider.Should().BeOfType<EncryptedFileSecretsProvider>();
    }

    [Fact]
    public void Create_File_WithoutPasswordCallback_ThrowsSecretsProviderException()
    {
        // Arrange
        var factory = new SecretsProviderFactory(
            NullLogger<SecretsProviderFactory>.Instance, NullLoggerFactory.Instance);

        // Act
        var act = () => factory.Create("file");

        // Assert
        act.Should().Throw<SmartHalSecretsProviderException>();
    }

    [Fact]
    public void Create_UnknownProvider_ThrowsSecretsProviderException()
    {
        // Arrange
        var factory = new SecretsProviderFactory(
            NullLogger<SecretsProviderFactory>.Instance, NullLoggerFactory.Instance);

        // Act
        var act = () => factory.Create("unknown");

        // Assert
        act.Should().Throw<SmartHalSecretsProviderException>();
    }

    [Fact]
    public void Create_Auto_OnMacOs_ReturnsMacOsProvider()
    {
        if (!OperatingSystem.IsMacOS())
        {
            return; // Skip on non-macOS
        }

        // Arrange
        var factory = new SecretsProviderFactory(
            NullLogger<SecretsProviderFactory>.Instance, NullLoggerFactory.Instance);

        // Act
        var provider = factory.Create("auto");

        // Assert
        provider.Should().BeOfType<MacOsKeychainProvider>();
    }

    [Fact]
    public void Create_Windows_OnNonWindows_ThrowsPlatformNotSupportedException()
    {
        if (OperatingSystem.IsWindows())
        {
            return; // Skip on Windows
        }

        // Arrange
        var factory = new SecretsProviderFactory(
            NullLogger<SecretsProviderFactory>.Instance, NullLoggerFactory.Instance);

        // Act
        var act = () => factory.Create("windows");

        // Assert
        act.Should().Throw<PlatformNotSupportedException>();
    }

    [Fact]
    public void Create_Linux_OnNonLinux_ThrowsPlatformNotSupportedException()
    {
        if (OperatingSystem.IsLinux())
        {
            return; // Skip on Linux
        }

        // Arrange
        var factory = new SecretsProviderFactory(
            NullLogger<SecretsProviderFactory>.Instance, NullLoggerFactory.Instance);

        // Act
        var act = () => factory.Create("linux");

        // Assert
        act.Should().Throw<PlatformNotSupportedException>();
    }
}
