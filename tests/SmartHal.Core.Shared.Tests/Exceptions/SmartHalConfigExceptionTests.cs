using AwesomeAssertions;

namespace SmartHal.Core.Exceptions;

public class SmartHalConfigExceptionTests
{
    [Fact]
    public void ConfigFileException_StoresFilePath()
    {
        // Act
        var ex = new SmartHalConfigFileException("parse error", "/etc/config.yaml");

        // Assert
        ex.FilePath.Should().Be("/etc/config.yaml");
        ex.LineNumber.Should().BeNull();
    }

    [Fact]
    public void ConfigFileException_StoresFilePathAndLineNumber()
    {
        // Act
        var ex = new SmartHalConfigFileException("parse error", "/etc/config.yaml", 42);

        // Assert
        ex.FilePath.Should().Be("/etc/config.yaml");
        ex.LineNumber.Should().Be(42);
    }

    [Fact]
    public void ConfigValidationException_StoresValidationErrors()
    {
        // Arrange
        var errors = new List<string> { "Field X is required", "Field Y is invalid" };

        // Act
        var ex = new SmartHalConfigValidationException("validation failed", errors);

        // Assert
        ex.ValidationErrors.Should().HaveCount(2);
        ex.ValidationErrors.Should().Contain("Field X is required");
        ex.ValidationErrors.Should().Contain("Field Y is invalid");
    }
}
