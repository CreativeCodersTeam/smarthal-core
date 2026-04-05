using AwesomeAssertions;

namespace SmartHal.Core.Exceptions;

public class SmartHalSecretsExceptionTests
{
    [Fact]
    public void SecretNotFoundException_FormatsMessage()
    {
        var ex = new SmartHalSecretNotFoundException("api-key");

        ex.Message.Should().Be("Secret 'api-key' was not found.");
        ex.SecretKey.Should().Be("api-key");
    }

    [Fact]
    public void SecretsProviderException_StoresProviderName()
    {
        var ex = new SmartHalSecretsProviderException("connection failed", "azure-keyvault");

        ex.ProviderName.Should().Be("azure-keyvault");
    }

    [Fact]
    public void SecretsProviderException_WithInnerException_SetsBoth()
    {
        var inner = new InvalidOperationException("inner");

        var ex = new SmartHalSecretsProviderException("error", "env-provider", inner);

        ex.ProviderName.Should().Be("env-provider");
        ex.InnerException.Should().BeSameAs(inner);
    }
}
