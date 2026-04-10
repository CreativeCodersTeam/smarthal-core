using SmartHal.Core.Logging;
using AwesomeAssertions;

namespace SmartHal.CLI.Infrastructure;

public class CliContextTests
{
    [Fact]
    public void ParseGlobalOptions_ConfigPath_SetsPathAndRemovesFromArgs()
    {
        // Arrange
        var context = new CliContext();

        // Act
        var remaining = context.ParseGlobalOptions(["--config-path", "/tmp/test", "device", "list"]);

        // Assert
        context.ConfigPath.Should().Be("/tmp/test");
        remaining.Should().BeEquivalentTo(["device", "list"]);
    }

    [Theory]
    [InlineData("-v", LogVerbosity.Debug)]
    [InlineData("-vv", LogVerbosity.Verbose)]
    [InlineData("--quiet", LogVerbosity.Quiet)]
    public void ParseGlobalOptions_VerbosityFlag_SetsCorrectVerbosity(string flag, LogVerbosity expected)
    {
        // Arrange
        var context = new CliContext();

        // Act
        context.ParseGlobalOptions([flag, "device", "list"]);

        // Assert
        context.Verbosity.Should().Be(expected);
    }

    [Theory]
    [InlineData("json", OutputFormat.Json)]
    [InlineData("yaml", OutputFormat.Yaml)]
    [InlineData("table", OutputFormat.Table)]
    [InlineData("xml", OutputFormat.Table)]
    public void ParseGlobalOptions_OutputFlag_SetsCorrectFormat(string format, OutputFormat expected)
    {
        // Arrange
        var context = new CliContext();

        // Act
        context.ParseGlobalOptions(["--output", format, "device", "list"]);

        // Assert
        context.OutputFormat.Should().Be(expected);
    }

    [Fact]
    public void ParseGlobalOptions_NoGlobalOptions_ReturnsAllArgs()
    {
        // Arrange
        var context = new CliContext();

        // Act
        var remaining = context.ParseGlobalOptions(["device", "list", "--adapter", "hm1"]);

        // Assert
        context.Verbosity.Should().Be(LogVerbosity.Normal);
        context.OutputFormat.Should().Be(OutputFormat.Table);
        remaining.Should().BeEquivalentTo(["device", "list", "--adapter", "hm1"]);
    }

    [Fact]
    public void ParseGlobalOptions_MultipleOptions_AllParsed()
    {
        // Arrange
        var context = new CliContext();

        // Act
        var remaining = context.ParseGlobalOptions(
            ["--config-path", "/tmp", "-v", "--output", "json", "device", "list"]);

        // Assert
        context.ConfigPath.Should().Be("/tmp");
        context.Verbosity.Should().Be(LogVerbosity.Debug);
        context.OutputFormat.Should().Be(OutputFormat.Json);
        remaining.Should().BeEquivalentTo(["device", "list"]);
    }
}
