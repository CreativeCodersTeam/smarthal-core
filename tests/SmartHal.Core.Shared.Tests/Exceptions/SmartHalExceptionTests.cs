using AwesomeAssertions;

namespace SmartHal.Core.Exceptions;

public class SmartHalExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        var ex = new SmartHalException("test error");

        ex.Message.Should().Be("test error");
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsBoth()
    {
        var inner = new InvalidOperationException("inner");

        var ex = new SmartHalException("outer", inner);

        ex.Message.Should().Be("outer");
        ex.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void AllExceptions_DeriveFrom_SmartHalException()
    {
        typeof(SmartHalConfigException).Should().BeDerivedFrom<SmartHalException>();
        typeof(SmartHalConfigFileException).Should().BeDerivedFrom<SmartHalException>();
        typeof(SmartHalConfigValidationException).Should().BeDerivedFrom<SmartHalException>();
        typeof(SmartHalAdapterException).Should().BeDerivedFrom<SmartHalException>();
        typeof(SmartHalAdapterConnectionException).Should().BeDerivedFrom<SmartHalException>();
        typeof(SmartHalAdapterDeviceNotFoundException).Should().BeDerivedFrom<SmartHalException>();
        typeof(SmartHalAdapterOperationException).Should().BeDerivedFrom<SmartHalException>();
        typeof(SmartHalSecretsException).Should().BeDerivedFrom<SmartHalException>();
        typeof(SmartHalSecretNotFoundException).Should().BeDerivedFrom<SmartHalException>();
        typeof(SmartHalSecretsProviderException).Should().BeDerivedFrom<SmartHalException>();
    }
}
