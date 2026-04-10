using SmartHal.Core.Logging;
using AwesomeAssertions;

namespace SmartHal.CLI.Infrastructure;

public class CliContextTests
{
    [Fact]
    public void ParseGlobalOptions_ConfigPath_SetsPathAndRemovesFromArgs()
    {
        var context = new CliContext();

        var remaining = context.ParseGlobalOptions(["--config-path", "/tmp/test", "device", "list"]);

        context.ConfigPath.Should().Be("/tmp/test");
        remaining.Should().BeEquivalentTo(["device", "list"]);
    }

    [Fact]
    public void ParseGlobalOptions_Verbose_SetsDebugVerbosity()
    {
        var context = new CliContext();

        var remaining = context.ParseGlobalOptions(["-v", "config", "validate"]);

        context.Verbosity.Should().Be(LogVerbosity.Debug);
        remaining.Should().BeEquivalentTo(["config", "validate"]);
    }

    [Fact]
    public void ParseGlobalOptions_VeryVerbose_SetsVerboseLevel()
    {
        var context = new CliContext();

        var remaining = context.ParseGlobalOptions(["-vv", "device", "list"]);

        context.Verbosity.Should().Be(LogVerbosity.Verbose);
        remaining.Should().BeEquivalentTo(["device", "list"]);
    }

    [Fact]
    public void ParseGlobalOptions_Quiet_SetsQuietVerbosity()
    {
        var context = new CliContext();

        var remaining = context.ParseGlobalOptions(["--quiet", "backup", "list"]);

        context.Verbosity.Should().Be(LogVerbosity.Quiet);
        remaining.Should().BeEquivalentTo(["backup", "list"]);
    }

    [Fact]
    public void ParseGlobalOptions_OutputJson_SetsJsonFormat()
    {
        var context = new CliContext();

        var remaining = context.ParseGlobalOptions(["--output", "json", "device", "list"]);

        context.OutputFormat.Should().Be(OutputFormat.Json);
        remaining.Should().BeEquivalentTo(["device", "list"]);
    }

    [Fact]
    public void ParseGlobalOptions_OutputYaml_SetsYamlFormat()
    {
        var context = new CliContext();

        var remaining = context.ParseGlobalOptions(["--output", "yaml", "device", "list"]);

        context.OutputFormat.Should().Be(OutputFormat.Yaml);
        remaining.Should().BeEquivalentTo(["device", "list"]);
    }

    [Fact]
    public void ParseGlobalOptions_NoGlobalOptions_ReturnsAllArgs()
    {
        var context = new CliContext();

        var remaining = context.ParseGlobalOptions(["device", "list", "--adapter", "hm1"]);

        context.Verbosity.Should().Be(LogVerbosity.Normal);
        context.OutputFormat.Should().Be(OutputFormat.Table);
        remaining.Should().BeEquivalentTo(["device", "list", "--adapter", "hm1"]);
    }

    [Fact]
    public void ParseGlobalOptions_MultipleOptions_AllParsed()
    {
        var context = new CliContext();

        var remaining = context.ParseGlobalOptions(
            ["--config-path", "/tmp", "-v", "--output", "json", "device", "list"]);

        context.ConfigPath.Should().Be("/tmp");
        context.Verbosity.Should().Be(LogVerbosity.Debug);
        context.OutputFormat.Should().Be(OutputFormat.Json);
        remaining.Should().BeEquivalentTo(["device", "list"]);
    }

    [Fact]
    public void ParseGlobalOptions_UnknownOutputFormat_DefaultsToTable()
    {
        var context = new CliContext();

        context.ParseGlobalOptions(["--output", "xml"]);

        context.OutputFormat.Should().Be(OutputFormat.Table);
    }
}
