using AwesomeAssertions;

namespace SmartHal.Core.Exceptions;

public class SmartHalSecretsExceptionTests
{
    [Fact]
    public void SecretNotFoundException_FormatsMessage()
    {
        // Act
        var ex = new SmartHalSecretNotFoundException("api-key");

        // Assert
        ex.Message.Should().Be("Secret 'api-key' was not found.");
        ex.SecretKey.Should().Be("api-key");
    }

    [Fact]
    public void SecretsProviderException_StoresProviderName()
    {
        // Act
        var ex = new SmartHalSecretsProviderException("connection failed", "azure-keyvault");

        // Assert
        ex.ProviderName.Should().Be("azure-keyvault");
    }

    [Fact]
    public void SecretsProviderException_WithInnerException_SetsBoth()
    {
        // Arrange
        var inner = new InvalidOperationException("inner");

        // Act
        var ex = new SmartHalSecretsProviderException("error", "env-provider", inner);

        // Assert
        ex.ProviderName.Should().Be("env-provider");
        ex.InnerException.Should().BeSameAs(inner);
    }
}
