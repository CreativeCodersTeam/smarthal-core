using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace SmartHal.Core.Secrets;

public class EnvironmentVariableSecretsProviderTests : IDisposable
{
    private const string TestKey = "test-adapter.test_secret";
    private const string TestEnvVar = "SMARTHAL_TEST_ADAPTER_TEST_SECRET";

    public EnvironmentVariableSecretsProviderTests()
    {
        Environment.SetEnvironmentVariable(TestEnvVar, "test-value");
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(TestEnvVar, null);
    }

    [Fact]
    public void ProviderName_IsEnv()
    {
        // Arrange
        var provider = new EnvironmentVariableSecretsProvider(
            NullLogger<EnvironmentVariableSecretsProvider>.Instance);

        // Act & Assert
        provider.ProviderName.Should().Be("env");
    }

    [Fact]
    public async Task GetSecretAsync_ExistingKey_ReturnsValue()
    {
        // Arrange
        var provider = new EnvironmentVariableSecretsProvider(
            NullLogger<EnvironmentVariableSecretsProvider>.Instance);

        // Act
        var value = await provider.GetSecretAsync(TestKey);

        // Assert
        value.Should().Be("test-value");
    }

    [Fact]
    public async Task GetSecretAsync_MissingKey_ThrowsSecretNotFoundException()
    {
        // Arrange
        var provider = new EnvironmentVariableSecretsProvider(
            NullLogger<EnvironmentVariableSecretsProvider>.Instance);

        // Act
        var act = () => provider.GetSecretAsync("nonexistent.key");

        // Assert
        await act.Should().ThrowAsync<SmartHalSecretNotFoundException>();
    }

    [Fact]
    public async Task ExistsAsync_ExistingKey_ReturnsTrue()
    {
        // Arrange
        var provider = new EnvironmentVariableSecretsProvider(
            NullLogger<EnvironmentVariableSecretsProvider>.Instance);

        // Act
        var exists = await provider.ExistsAsync(TestKey);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_MissingKey_ReturnsFalse()
    {
        // Arrange
        var provider = new EnvironmentVariableSecretsProvider(
            NullLogger<EnvironmentVariableSecretsProvider>.Instance);

        // Act
        var exists = await provider.ExistsAsync("nonexistent.key");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task SetSecretAsync_ThrowsSecretsProviderException()
    {
        // Arrange
        var provider = new EnvironmentVariableSecretsProvider(
            NullLogger<EnvironmentVariableSecretsProvider>.Instance);

        // Act
        var act = () => provider.SetSecretAsync("any.key", "value");

        // Assert
        await act.Should().ThrowAsync<SmartHalSecretsProviderException>();
    }

    [Fact]
    public async Task DeleteSecretAsync_ThrowsSecretsProviderException()
    {
        // Arrange
        var provider = new EnvironmentVariableSecretsProvider(
            NullLogger<EnvironmentVariableSecretsProvider>.Instance);

        // Act
        var act = () => provider.DeleteSecretAsync("any.key");

        // Assert
        await act.Should().ThrowAsync<SmartHalSecretsProviderException>();
    }

    [Fact]
    public async Task ListKeysAsync_ReturnsKeysWithPrefix()
    {
        // Arrange
        var provider = new EnvironmentVariableSecretsProvider(
            NullLogger<EnvironmentVariableSecretsProvider>.Instance);

        // Act
        var keys = await provider.ListKeysAsync();

        // Assert
        keys.Should().Contain("test_adapter_test_secret");
    }
}
