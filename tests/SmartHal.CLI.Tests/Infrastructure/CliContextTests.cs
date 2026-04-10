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

    [Fact]
    public void ParseGlobalOptions_Verbose_SetsDebugVerbosity()
    {
        // Arrange
        var context = new CliContext();

        // Act
        var remaining = context.ParseGlobalOptions(["-v", "config", "validate"]);

        // Assert
        context.Verbosity.Should().Be(LogVerbosity.Debug);
        remaining.Should().BeEquivalentTo(["config", "validate"]);
    }

    [Fact]
    public void ParseGlobalOptions_VeryVerbose_SetsVerboseLevel()
    {
        // Arrange
        var context = new CliContext();

        // Act
        var remaining = context.ParseGlobalOptions(["-vv", "device", "list"]);

        // Assert
        context.Verbosity.Should().Be(LogVerbosity.Verbose);
        remaining.Should().BeEquivalentTo(["device", "list"]);
    }

    [Fact]
    public void ParseGlobalOptions_Quiet_SetsQuietVerbosity()
    {
        // Arrange
        var context = new CliContext();

        // Act
        var remaining = context.ParseGlobalOptions(["--quiet", "backup", "list"]);

        // Assert
        context.Verbosity.Should().Be(LogVerbosity.Quiet);
        remaining.Should().BeEquivalentTo(["backup", "list"]);
    }

    [Fact]
    public void ParseGlobalOptions_OutputJson_SetsJsonFormat()
    {
        // Arrange
        var context = new CliContext();

        // Act
        var remaining = context.ParseGlobalOptions(["--output", "json", "device", "list"]);

        // Assert
        context.OutputFormat.Should().Be(OutputFormat.Json);
        remaining.Should().BeEquivalentTo(["device", "list"]);
    }

    [Fact]
    public void ParseGlobalOptions_OutputYaml_SetsYamlFormat()
    {
        // Arrange
        var context = new CliContext();

        // Act
        var remaining = context.ParseGlobalOptions(["--output", "yaml", "device", "list"]);

        // Assert
        context.OutputFormat.Should().Be(OutputFormat.Yaml);
        remaining.Should().BeEquivalentTo(["device", "list"]);
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

    [Fact]
    public void ParseGlobalOptions_UnknownOutputFormat_DefaultsToTable()
    {
        // Arrange
        var context = new CliContext();

        // Act
        context.ParseGlobalOptions(["--output", "xml"]);

        // Assert
        context.OutputFormat.Should().Be(OutputFormat.Table);
    }
}
