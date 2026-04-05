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
        LogSetup.MapVerbosityToLevel(verbosity).Should().Be(expectedLevel);
    }

    [Fact]
    public void Configure_ReturnsNonNullLogger()
    {
        var logger = LogSetup.Configure(LogVerbosity.Normal);

        logger.Should().NotBeNull();
    }

    [Fact]
    public void Configure_WithFilePath_ReturnsNonNullLogger()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"smarthal-test-{Guid.NewGuid()}.log");

        try
        {
            var logger = LogSetup.Configure(LogVerbosity.Debug, tempPath);

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
}
