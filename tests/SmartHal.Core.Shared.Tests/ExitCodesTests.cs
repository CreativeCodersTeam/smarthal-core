using AwesomeAssertions;

namespace SmartHal.Core;

public class ExitCodesTests
{
    [Theory]
    [InlineData(typeof(SmartHalConfigFileException), ExitCodes.ConfigFileError)]
    [InlineData(typeof(SmartHalConfigValidationException), ExitCodes.ConfigValidationError)]
    [InlineData(typeof(SmartHalConfigException), ExitCodes.ConfigError)]
    [InlineData(typeof(SmartHalAdapterConnectionException), ExitCodes.AdapterConnectionError)]
    [InlineData(typeof(SmartHalAdapterDeviceNotFoundException), ExitCodes.AdapterDeviceNotFound)]
    [InlineData(typeof(SmartHalAdapterOperationException), ExitCodes.AdapterOperationError)]
    [InlineData(typeof(SmartHalAdapterException), ExitCodes.AdapterError)]
    [InlineData(typeof(SmartHalSecretNotFoundException), ExitCodes.SecretNotFound)]
    [InlineData(typeof(SmartHalSecretsProviderException), ExitCodes.SecretsProviderError)]
    [InlineData(typeof(SmartHalSecretsException), ExitCodes.SecretsError)]
    public void FromException_ReturnsCorrectCode(Type exceptionType, int expectedCode)
    {
        var ex = CreateException(exceptionType);

        ExitCodes.FromException(ex).Should().Be(expectedCode);
    }

    [Fact]
    public void FromException_SmartHalException_ReturnsGeneralError()
    {
        var ex = new SmartHalException("general error");

        ExitCodes.FromException(ex).Should().Be(ExitCodes.GeneralError);
    }

    [Fact]
    public void FromException_UnknownException_ReturnsGeneralError()
    {
        var ex = new InvalidOperationException("unknown");

        ExitCodes.FromException(ex).Should().Be(ExitCodes.GeneralError);
    }

    [Fact]
    public void FromException_SpecificExceptions_TakePrecedenceOverBaseTypes()
    {
        var fileEx = new SmartHalConfigFileException("error", "/path");
        var configEx = new SmartHalConfigException("error");

        ExitCodes.FromException(fileEx).Should().Be(ExitCodes.ConfigFileError);
        ExitCodes.FromException(configEx).Should().Be(ExitCodes.ConfigError);
        ExitCodes.ConfigFileError.Should().NotBe(ExitCodes.ConfigError);
    }

    private static Exception CreateException(Type type) => type.Name switch
    {
        nameof(SmartHalConfigFileException) => new SmartHalConfigFileException("error", "/path"),
        nameof(SmartHalConfigValidationException) => new SmartHalConfigValidationException("error", []),
        nameof(SmartHalConfigException) => new SmartHalConfigException("error"),
        nameof(SmartHalAdapterConnectionException) => new SmartHalAdapterConnectionException("error", "adapter"),
        nameof(SmartHalAdapterDeviceNotFoundException) => new SmartHalAdapterDeviceNotFoundException("error", "adapter", "native"),
        nameof(SmartHalAdapterOperationException) => new SmartHalAdapterOperationException("error", "adapter"),
        nameof(SmartHalAdapterException) => new SmartHalAdapterException("error", "adapter"),
        nameof(SmartHalSecretNotFoundException) => new SmartHalSecretNotFoundException("key"),
        nameof(SmartHalSecretsProviderException) => new SmartHalSecretsProviderException("error", "provider"),
        nameof(SmartHalSecretsException) => new SmartHalSecretsException("error"),
        _ => throw new ArgumentException($"Unknown exception type: {type.Name}")
    };
}
