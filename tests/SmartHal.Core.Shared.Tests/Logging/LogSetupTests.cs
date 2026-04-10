using AwesomeAssertions;
using Serilog.Events;
using SmartHal.Core.Logging;

namespace SmartHal.Core;

public class LogSetupTests
{
    [Theory]
    [InlineData(LogVerbosity.Quiet, LogEventLevel.Warning)]
    [InlineData(LogVerbosity.Normal, LogEventLevel.Information)]
    [InlineData(LogVerbosity.Debug, LogEventLevel.Debug)]
    [InlineData(LogVerbosity.Verbose, LogEventLevel.Verbose)]
    public void MapVerbosityToLevel_ReturnsCorrectLevel(LogVerbosity verbosity, LogEventLevel expectedLevel)
    {
        // Act & Assert
        LogSetup.MapVerbosityToLevel(verbosity).Should().Be(expectedLevel);
    }

    [Fact]
    public void Configure_ReturnsNonNullLogger()
    {
        // Act
        var logger = LogSetup.Configure(LogVerbosity.Normal);

        // Assert
        logger.Should().NotBeNull();
    }

    [Fact]
    public void Configure_WithFilePath_ReturnsNonNullLogger()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"smarthal-test-{Guid.NewGuid()}.log");

        try
        {
            // Act
            var logger = LogSetup.Configure(LogVerbosity.Debug, tempPath);

            // Assert
            logger.Should().NotBeNull();
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    [Fact]
    public void MapVerbosityToLevel_UnknownValue_ReturnsInformation()
    {
        // Act
        var level = LogSetup.MapVerbosityToLevel((LogVerbosity)999);

        // Assert
        level.Should().Be(LogEventLevel.Information);
    }
}
