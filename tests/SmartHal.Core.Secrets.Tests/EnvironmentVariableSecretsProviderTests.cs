using AwesomeAssertions;

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
        var provider = new EnvironmentVariableSecretsProvider();

        provider.ProviderName.Should().Be("env");
    }

    [Fact]
    public async Task GetSecretAsync_ExistingKey_ReturnsValue()
    {
        var provider = new EnvironmentVariableSecretsProvider();

        var value = await provider.GetSecretAsync(TestKey);

        value.Should().Be("test-value");
    }

    [Fact]
    public async Task GetSecretAsync_MissingKey_ThrowsSecretNotFoundException()
    {
        var provider = new EnvironmentVariableSecretsProvider();

        var act = () => provider.GetSecretAsync("nonexistent.key");

        await act.Should().ThrowAsync<SmartHalSecretNotFoundException>();
    }

    [Fact]
    public async Task ExistsAsync_ExistingKey_ReturnsTrue()
    {
        var provider = new EnvironmentVariableSecretsProvider();

        var exists = await provider.ExistsAsync(TestKey);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_MissingKey_ReturnsFalse()
    {
        var provider = new EnvironmentVariableSecretsProvider();

        var exists = await provider.ExistsAsync("nonexistent.key");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task SetSecretAsync_ThrowsSecretsProviderException()
    {
        var provider = new EnvironmentVariableSecretsProvider();

        var act = () => provider.SetSecretAsync("any.key", "value");

        await act.Should().ThrowAsync<SmartHalSecretsProviderException>();
    }

    [Fact]
    public async Task DeleteSecretAsync_ThrowsSecretsProviderException()
    {
        var provider = new EnvironmentVariableSecretsProvider();

        var act = () => provider.DeleteSecretAsync("any.key");

        await act.Should().ThrowAsync<SmartHalSecretsProviderException>();
    }

    [Fact]
    public async Task ListKeysAsync_ReturnsKeysWithPrefix()
    {
        var provider = new EnvironmentVariableSecretsProvider();

        var keys = await provider.ListKeysAsync();

        keys.Should().Contain("test_adapter_test_secret");
    }
}
