using AwesomeAssertions;

namespace SmartHal.Core.Exceptions;

public class SmartHalConfigExceptionTests
{
    [Fact]
    public void ConfigFileException_StoresFilePath()
    {
        var ex = new SmartHalConfigFileException("parse error", "/etc/config.yaml");

        ex.FilePath.Should().Be("/etc/config.yaml");
        ex.LineNumber.Should().BeNull();
    }

    [Fact]
    public void ConfigFileException_StoresFilePathAndLineNumber()
    {
        var ex = new SmartHalConfigFileException("parse error", "/etc/config.yaml", 42);

        ex.FilePath.Should().Be("/etc/config.yaml");
        ex.LineNumber.Should().Be(42);
    }

    [Fact]
    public void ConfigValidationException_StoresValidationErrors()
    {
        var errors = new List<string> { "Field X is required", "Field Y is invalid" };

        var ex = new SmartHalConfigValidationException("validation failed", errors);

        ex.ValidationErrors.Should().HaveCount(2);
        ex.ValidationErrors.Should().Contain("Field X is required");
        ex.ValidationErrors.Should().Contain("Field Y is invalid");
    }
}
